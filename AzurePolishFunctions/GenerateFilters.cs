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
                EconomyData.CreateSubEconomyTiers();

                ConcreteEnrichmentProcedures.Initialize();
                EconomyData.EnrichAll(EnrichmentProcedureConfiguration.PriorityEnrichmentProcedures);
                FilterPolishUtil.FilterPolishConfig.AdjustPricingInformation();
                EconomyData.EnrichAll(EnrichmentProcedureConfiguration.EnrichmentProcedures);
                
                // EconomyData.PerformClassAbstractionProcedures();

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
    }
}
