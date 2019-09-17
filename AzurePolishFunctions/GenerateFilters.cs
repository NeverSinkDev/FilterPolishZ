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

namespace AzurePolishFunctions
{
    public static class GenerateFilters
    {
        public static EconomyRequestFacade EconomyData { get; set; } = EconomyRequestFacade.GetInstance();
        public static ItemInformationFacade ItemInfoData { get; set; }
        public static TierListFacade TierListFacade { get; set; } = TierListFacade.GetInstance();
        public static FilterAccessFacade FilterAccessFacade { get; set; } = FilterAccessFacade.GetInstance();
        public static DataFileRequestFacade DataFiles { get; set; }

        [FunctionName("GenerateFilters")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Admin, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            return PerformMainRoutine();
        }

        public static IActionResult PerformMainRoutine()
        {
            // 0) Establish Logging, Facades

            // 0) Get Current League information etc
            // 1) Acquire Data
            if (DataFiles == null)
            {
                // todo: handling for repeated function calls after static vars already initialized
                DataFiles = new DataFileRequestFacade();
                DataFiles.GetAllFiles("Standard");
            }
            
            // 2) Test Data
            // 3) Initialize static enrichment information
            // 4) Parse filter, Load All files (Economy, Basetype, Tierlist) -> All facade

            FilterAccessFacade.PrimaryFilter = new Filter(DataFiles.SeedFilter);

            ConcreteEnrichmentProcedures.Initialize();
            EconomyData.EnrichAll(EnrichmentProcedureConfiguration.EnrichmentProcedures);
            TierListFacade.TierListData.Values.ToList().ForEach(x => x.ReEvaluate());

            // 5) Generate Suggestions

            var economyTieringSystem = new ConcreteEconomyRules();
            TierListFacade.TierListData = FilterAccessFacade.PrimaryFilter.ExtractTiers(FilterPolishConfig.FilterTierLists);
            economyTieringSystem.GenerateSuggestions();
            TierListFacade.TierListData.Values.ToList().ForEach(x => x.ReEvaluate());

            // 6) Apply suggestions

            TierListFacade.ApplyAllSuggestions();

            // 7) Generate Filters
            // 8) Generate changelogs
            // 9) Upload filters
            
            return new OkObjectResult("successfully generated filters");
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
            EconomyData.AddToDictionary("rare->normal", otherbases);
        }
    }
}
