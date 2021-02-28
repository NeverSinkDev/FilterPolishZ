using AzurePolishFunctions.DataFileRequests;
using FilterCore;
using FilterCore.Constants;
using FilterEconomy.Facades;
using FilterEconomyProcessor;
using FilterPolishUtil;
using FilterPolishUtil.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzurePolishFunctions.Procedures
{

    public class MainGenerationRoutine
    {
        public static EconomyRequestFacade EconomyData { get; set; }
        public static ItemInformationFacade ItemInfoData { get; set; }
        public static TierListFacade TierListFacade { get; set; }
        public static FilterAccessFacade FilterAccessFacade { get; set; }
        public static DataFileRequestFacade DataFiles { get; set; }
        public static FilterPublisher Publisher { get; set; }

        public MainGenerationRoutine()
        {
            Logging = LoggingFacade.GetInstance();
        }

        public static LoggingFacade Logging { get; set; }

        public void Execute(string req)
        {
            // Logging?.Clean();
            Logging = LoggingFacade.GetInstance();

            EconomyData?.Clean();
            ItemInfoData?.Clean();
            TierListFacade?.Clean();
            FilterAccessFacade?.Clean();

            BaseTypeDataProvider.Initialize();
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

            var localMode = Environment.GetEnvironmentVariable("localMode", EnvironmentVariableTarget.Process) ?? "true";
            var requestedLeagueName = EconomyData.GetActiveLeagueName();

            // 1) Acquire Data
            dynamic data = JsonConvert.DeserializeObject(req);
            string leagueType = data.leagueType ?? Environment.GetEnvironmentVariable("leagueType", EnvironmentVariableTarget.Process) ?? "tmpstandard";
            string repoName = data.repoName ?? Environment.GetEnvironmentVariable("repoName", EnvironmentVariableTarget.Process) ?? "NeverSink-EconomyUpdated-Filter";
            string league = requestedLeagueName; //GetReqParams(req, data, "currentLeague", "Metamorph");

            LoggingFacade.LogInfo($"[CONFIG] leagueType: {leagueType}");
            LoggingFacade.LogInfo($"[CONFIG] league: {league}");
            LoggingFacade.LogInfo($"[CONFIG] repoName: {repoName}");
            LoggingFacade.LogInfo($"[CONFIG] localMode: {localMode}");

            FilterPolishConfig.ApplicationExecutionMode = ExecutionMode.Function;

            DataFiles = new DataFileRequestFacade();

            if (localMode == "true")
            {
                FilterPolishConfig.ActiveRequestMode = RequestType.Dynamic;
                DataFiles.BaseStoragePath = @"C:\FilterOutput\EcoData";
            }
            else
            {
                FilterPolishConfig.ActiveRequestMode = RequestType.ForceOnline;
            }
            
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
            if (dataRes == FileRequestResult.Success && EconomyData.IsLeagueActive())
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
                
                // Generate Suggestions
                var economyTieringSystem = new ConcreteEconomyRules();
                economyTieringSystem.GenerateSuggestions();
                // Apply suggestions
                TierListFacade.ApplyAllSuggestions();
            }

            LoggingFacade.LogInfo($"[DEBUG] Seedfiler regeneration done. Starting generation...");

            // Generate and Upload Filters
            Publisher = new FilterPublisher(FilterAccessFacade.PrimaryFilter, repoName, leagueType);

            LoggingFacade.LogInfo($"[DEBUG] Initializing Publisher...");
            Publisher.Init(dataRes);
            LoggingFacade.LogInfo($"[DEBUG] Filter Generation Done!");

            LoggingFacade.LogInfo($"[DEBUG] LadderPublishing:");
            Publisher.PublishToLadder();

            LoggingFacade.LogInfo($"[DEBUG] GitHub:");
            //Publisher.PublishToGitHub();

            LoggingFacade.LogInfo($"[DEBUG] FilterBlade:");
            Publisher.PublishToFilterBlade();

            LoggingFacade.LogInfo($"[DEBUG] FilterBlade Beta:");
            Publisher.PublishToFilterBladeBETA();
        }
    }
}
