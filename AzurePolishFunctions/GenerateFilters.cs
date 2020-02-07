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
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

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
        public static string Run([ActivityTrigger] string req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            Logging?.Clean();
            Logging = LoggingFacade.GetInstance();

            Logging.SetCustomLoggingMessage((s) => log.LogInformation(s));

            try
            {
                PerformMainRoutine(req);
                return "finished generation succesfully!";
            }
            catch (Exception e)
            {
                Logging.Log(e.Message, LoggingLevel.Errors);
                return e.Message;
            }
        }

        public static void PerformMainRoutine(string req)
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

            EconomyData.RequestPoeLeagueInfo();

            if (!EconomyData.IsLeagueActive())
            {
                LoggingFacade.LogWarning("No Active League detected!");
            }

            var requestedLeagueName = EconomyData.GetActiveLeagueName();

            // 1) Acquire Data

            var localMode = Environment.GetEnvironmentVariable("localMode", EnvironmentVariableTarget.Process) ?? "true";

            // string body = new StreamReader(req.Body).ReadToEnd();
            // var repoName = GetReqParams(req, data, "repoName", "NeverSink-EconomyUpdated-Filter");
            // var leagueType = GetReqParams(req, data.leagueType, "leagueType", "tmpstandard");

            dynamic data = JsonConvert.DeserializeObject(req);

            string leagueType = data.leagueType ?? "tmpstandard";
            string repoName = data.repoName ?? "NeverSink-EconomyUpdated-Filter";

            string league = requestedLeagueName; //GetReqParams(req, data, "currentLeague", "Metamorph");

            LoggingFacade.LogInfo($"[CONFIG] leagueType: {leagueType}");
            LoggingFacade.LogInfo($"[CONFIG] league: {league}");
            LoggingFacade.LogInfo($"[CONFIG] repoName: {repoName}");
            LoggingFacade.LogInfo($"[CONFIG] localMode: {localMode}");

            if (localMode == "true")
            {
                FilterPolishConfig.ActiveRequestMode = RequestType.Dynamic;
            }
            else
            {
                FilterPolishConfig.ActiveRequestMode = RequestType.ForceOnline;
            }

            DataFiles = new DataFileRequestFacade();
            LoggingFacade.LogInfo($"[CONFIG] FileRequestFacade Created!");

            FileRequestResult dataRes = DataFiles.GetAllFiles(league, leagueType);

            // 3) Parse filter
            FilterAccessFacade.PrimaryFilter = new Filter(DataFiles.SeedFilter);
            var newVersion = FilterAccessFacade.PrimaryFilter.GetHeaderMetaData("VERSION") + "." + DateTime.Now.Year + "." + DateTime.Now.DayOfYear + "." + DateTime.Now.Hour;
            FilterAccessFacade.PrimaryFilter.SetHeaderMetaData("VERSION", newVersion);

            LoggingFacade.LogInfo($"[CONFIG] version: {newVersion}");
            LoggingFacade.LogInfo($"[DEBUG] FileRequestResult: {dataRes.ToString()}");
            LoggingFacade.LogInfo($"[DEBUG] League Active: {EconomyData.IsLeagueActive().ToString()}");

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

            LoggingFacade.LogInfo($"[DEBUG] Seedfiler regeneration done. Starting publishing...");

            // 8) Generate and Upload Filters
            new FilterPublisher(FilterAccessFacade.PrimaryFilter, repoName).Run(dataRes);
        }

        private static string GetReqParams(string req, dynamic data, string name, string defValue)
        {
            string result = string.Empty;

            result = req;

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
