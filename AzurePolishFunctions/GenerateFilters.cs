using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FilterEconomy.Facades;
using FilterCore;
using FilterEconomyProcessor;
using System.Linq;
using System.Collections.Generic;
using FilterPolishUtil.Collections;
using FilterEconomy.Model;
using AzurePolishFunctions.DataFileRequests;
using FilterPolishUtil;
using FilterPolishUtil.Model;

namespace AzurePolishFunctions
{
    public static class GenerateFilters
    {
        public static EconomyRequestFacade EconomyData { get; set; } 
        public static ItemInformationFacade ItemInfoData { get; set; }
        public static TierListFacade TierListFacade { get; set; }
        public static FilterAccessFacade FilterAccessFacade { get; set; }
        public static DataFileRequestFacade DataFiles { get; set; }

        public static LoggingFacade Logging { get; set; }

        [FunctionName("GenerateFilters")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            Logging?.Clean();
            Logging = LoggingFacade.GetInstance();

            try
            {
                PerformMainRoutine();
                return new OkObjectResult("successfully generated filters");
            }
            catch (Exception e)
            {
                Logging.Log(e.Message, LoggingLevel.Errors);
                return new ConflictObjectResult(e);
            }
        }

        public static void PerformMainRoutine()
        {
            // 0) Cleanup
            EconomyData?.Clean();
            ItemInfoData?.Clean();
            TierListFacade?.Clean();
            FilterAccessFacade?.Clean();

            EconomyData = EconomyRequestFacade.GetInstance();
            TierListFacade = TierListFacade.GetInstance();
            FilterAccessFacade = FilterAccessFacade.GetInstance();
            ItemInfoData = ItemInformationFacade.GetInstance();

            // 0) Get Current League information etc
            // 1) Acquire Data
            var league = Environment.GetEnvironmentVariable("ninjaLeague", EnvironmentVariableTarget.Process) ?? "tmpstandard";
            var localMode = Environment.GetEnvironmentVariable("localMode", EnvironmentVariableTarget.Process) ?? "true";

            if (localMode == "true")
            {
                FilterPolishConfig.ActiveRequestMode = RequestType.Dynamic;
            }
            else
            {
                FilterPolishConfig.ActiveRequestMode = RequestType.ForceOnline;
            }

            DataFiles = new DataFileRequestFacade();
            DataFiles.GetAllFiles(league);

            // 2) Test Data
            // todo

            // 3) Parse filter
            FilterAccessFacade.PrimaryFilter = new Filter(DataFiles.SeedFilter);

            // 4) Load ier list information and enrichment procedures
            var tiers = FilterAccessFacade.PrimaryFilter.ExtractTiers(FilterPolishConfig.FilterTierLists);
            TierListFacade.TierListData = tiers;
            CreateSubEconomyTiers();

            ConcreteEnrichmentProcedures.Initialize();
            EconomyData.EnrichAll(EnrichmentProcedureConfiguration.PriorityEnrichmentProcedures);
            FilterPolishUtil.FilterPolishConfig.AdjustPricingInformation();
            EconomyData.EnrichAll(EnrichmentProcedureConfiguration.EnrichmentProcedures);

            TierListFacade.TierListData.Values.ToList().ForEach(x => x.ReEvaluate());

            // 5) Generate Suggestions
            var economyTieringSystem = new ConcreteEconomyRules();
            economyTieringSystem.GenerateSuggestions();

            // 6) Apply suggestions
            TierListFacade.ApplyAllSuggestions();

            // 7) Generate changelogs
            // todo
            
            // 8) Generate and Upload Filters
            new FilterPublisher(FilterAccessFacade.PrimaryFilter).Run();
        }

        private static void CreateSubEconomyTiers()
        {
            var shaperbases = new Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>>();
            var elderbases = new Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>>();
            var otherbases = new Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>>();

            foreach (var items in EconomyData.EconomyTierlistOverview["basetypes"])
            {
                var shapergroup = items.Value.Where(x => x.Variant == "Shaper").ToList();
                if (shapergroup.Count != 0)
                {
                    shaperbases.Add(items.Key, new ItemList<NinjaItem>());
                    shaperbases[items.Key].AddRange((shapergroup));
                }

                var eldegroup = items.Value.Where(x => x.Variant == "Elder").ToList();
                if (eldegroup.Count != 0)
                {
                    elderbases.Add(items.Key, new ItemList<NinjaItem>());
                    elderbases[items.Key].AddRange((eldegroup));
                }

                var othergroup = items.Value.Where(x => x.Variant != "Shaper" && x.Variant != "Elder").ToList();
                if (othergroup.Count != 0)
                {
                    otherbases.Add(items.Key, new ItemList<NinjaItem>());
                    otherbases[items.Key].AddRange((othergroup));
                }
            }

            EconomyData.AddToDictionary("rare->shaper", shaperbases);
            EconomyData.AddToDictionary("rare->elder", elderbases);
            EconomyData.AddToDictionary("generalcrafting", otherbases);
        }
    }
}
