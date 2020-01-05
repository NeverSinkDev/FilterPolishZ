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
                PerformMainRoutine(req);
                return new OkObjectResult("successfully generated filters");
            }
            catch (Exception e)
            {
                Logging.Log(e.Message, LoggingLevel.Errors);
                return new ConflictObjectResult(e);
            }
        }

        public static void PerformMainRoutine(HttpRequest req)
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
            
            var localMode = Environment.GetEnvironmentVariable("localMode", EnvironmentVariableTarget.Process) ?? "true";

            string body = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(body);

            var leagueType = GetReqParams(req, data, "leagueType", "tmpstandard");
            var league = GetReqParams(req, data, "currentLeague", "Metamorph");
            var repoName = GetReqParams(req, data, "repoName", "NeverSink-EconomyUpdated-Filter");

            if (localMode == "true")
            {
                FilterPolishConfig.ActiveRequestMode = RequestType.Dynamic;
            }
            else
            {
                FilterPolishConfig.ActiveRequestMode = RequestType.ForceOnline;
            }

            DataFiles = new DataFileRequestFacade();
            FileRequestResult dataRes = DataFiles.GetAllFiles(league, leagueType);

            // 2) Test Data
            // todo

            // 3) Parse filter
            FilterAccessFacade.PrimaryFilter = new Filter(DataFiles.SeedFilter);
            var newVersion = FilterAccessFacade.PrimaryFilter.GetHeaderMetaData("VERSION") + "." + DateTime.Now.Year + "." + DateTime.Now.DayOfYear + "." + DateTime.Now.Hour;
            FilterAccessFacade.PrimaryFilter.SetHeaderMetaData("VERSION", newVersion);

            // null check the ecoData in case of disabled/early leagues
            if (dataRes == FileRequestResult.Success)
            {
                // 4) Load tier list information and enrichment procedures
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
            }

            // 7) Generate changelogs
            // todo
            
            // 8) Generate and Upload Filters
            new FilterPublisher(FilterAccessFacade.PrimaryFilter, repoName).Run(dataRes);
        }

        private static string GetReqParams(HttpRequest req, dynamic data, string name, string defValue)
        {
            string result = string.Empty;

            result = req.Query[name];

            if (result != null && result != string.Empty)
            {
                return result;
            }

            result = data?[name];

            if (result != null && result != string.Empty)
            {
                return result;
            }

            result = Environment.GetEnvironmentVariable(name);
            if (result != null && result != string.Empty)
            {
                return result;
            }

            return defValue;
        }

        private static void CreateSubEconomyTiers()
        {
            List<string> influenceTypes = new List<string>() { "Shaper", "Elder", "Warlord", "Cursader", "Redeemer", "Hunter" };
            var metaDictionary = new Dictionary<string, Dictionary<string, ItemList<NinjaItem>>>();

            influenceTypes.ForEach(x => metaDictionary.Add(x, new Dictionary<string, ItemList<NinjaItem>>()));

            var shaperbases = new Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>>();
            var elderbases = new Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>>();
            var otherbases = new Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>>();

            foreach (var items in EconomyData.EconomyTierlistOverview["basetypes"])
            {
                foreach (var influence in influenceTypes)
                {
                    var influencedGroup = items.Value.Where(x => x.Variant == influence).ToList();
                    if (influencedGroup.Count != 0)
                    {
                        metaDictionary[influence].Add(items.Key, new ItemList<NinjaItem>());
                        metaDictionary[influence][items.Key].AddRange((influencedGroup));
                    }
                }

                var othergroup = items.Value.Where(x => !influenceTypes.Contains(x.Variant)).ToList();
                if (othergroup.Count != 0)
                {
                    otherbases.Add(items.Key, new ItemList<NinjaItem>());
                    otherbases[items.Key].AddRange((othergroup));
                }
            }

            EconomyData.AddToDictionary("rare->shaper", metaDictionary["Shaper"]);
            EconomyData.AddToDictionary("rare->elder", metaDictionary["Elder"]);
            EconomyData.AddToDictionary("rare->warlord", metaDictionary["Warlord"]);
            EconomyData.AddToDictionary("rare->crusader", metaDictionary["Crusader"]);
            EconomyData.AddToDictionary("rare->redeemer", metaDictionary["Redeemer"]);
            EconomyData.AddToDictionary("rare->hunter", metaDictionary["Hunter"]);
            EconomyData.AddToDictionary("generalcrafting", otherbases);

            LoggingFacade.LogInfo($"Done Generating Sub-Economy Tiers");
        }
    }
}
