using System;
using System.Collections.Generic;
using System.Linq;
using FilterEconomy.Model;
using FilterPolishUtil.Collections;

namespace AzurePolishFunctions.DataFileRequests
{
    public class DataFileRequestFacade
    {
        public Dictionary<string, List<string>> FilterStyleSheets { get; set; } = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> ItemAspects { get; set; } = new Dictionary<string, List<string>>();
        public Dictionary<string, Dictionary<string, string>> BaseTypeData { get; set; }
        public Dictionary<string, Dictionary<string, ItemList<NinjaItem>>> EconomyData { get; set; }
        public List<string> SeedFilter { get; set; }

        
        public void GetAllFiles(string ninjaLeague)
        {
            FilterPolishUtil.Model.LoggingFacade.GetInstance().CustomLoggingAction = x => Console.WriteLine("err: " + x);
            
            // Lade Filter Datei -> NS main github -> selective file(s?)
            // Lade Style Dateien -> NS main github -> selective files
            // Lade Aspect Dateien -> https://github.com/NeverSinkDev/Filter-ItemEconomyAspects -> ALL
            this.LoadGitHubFiles();
            
            // Lade BaseType Datei -> FB
            this.BaseTypeData = FilterCore.Constants.BaseTypeDataProvider.BaseTypeData;

            // Lade Economy Dateien -> ninja API
            this.LoadEcoData(ninjaLeague);
        }

        private void LoadEcoData(string ninjaLeague)
        {
            var result = FilterEconomy.Facades.EconomyRequestFacade.GetInstance();
            var ninjaUrl = "http://poe.ninja/api/Data/"; //Configuration.AppSettings["Ninja Request URL"];
            var variation = ninjaLeague; //Configuration.AppSettings["Ninja League"];
            var league = ""; // Configuration.AppSettings["betrayal"];
            var requestMode = FilterEconomy.Facades.EconomyRequestFacade.RequestType.ForceOnline;

            foreach (var tuple in FilterPolishUtil.FilterPolishConfig.FileRequestData)
            {
                PerformEcoRequest(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
            }

            void PerformEcoRequest(string dictionaryKey, string requestKey, string url, string prefix) =>
                result.AddToDictionary(dictionaryKey,
                    result.PerformRequest(league, variation, requestKey, url, prefix, requestMode, null, ninjaUrl));

            this.EconomyData = result.EconomyTierlistOverview;
        }

        private void LoadGitHubFiles()
        {
            var nsRepo = "NeverSink-Filter";
            var nsName = "NeverSinkDev";
            var seedFilterRepoPath = "/ADDITIONAL-FILES/SeedFilter/NeverSink's filter - SEED (SeedFilter) .filter";
            var styleFolderRepoPath = "/ADDITIONAL-FILES/StyleSheets/";
            var repoDlPath = new GitHubFileDownloader().Download(nsName, nsRepo);
            
            // seed filter
            var seedFilterContent = System.IO.File.ReadAllLines(repoDlPath + seedFilterRepoPath);
            this.SeedFilter = seedFilterContent.ToList();
            
            // filter styles
            System.IO.Directory
                .EnumerateFiles(repoDlPath + styleFolderRepoPath)
                .ToList()
                .ForEach(x => this.FilterStyleSheets.Add(System.IO.Path.GetFileName(x), System.IO.File.ReadAllLines(x).ToList()));
            
            // aspects
            System.IO.Directory
                .EnumerateFiles(new GitHubFileDownloader().Download(nsName, "Filter-ItemEconomyAspects"))
                .ToList()
                .ForEach(x => this.ItemAspects.Add(System.IO.Path.GetFileName(x), System.IO.File.ReadAllLines(x).ToList()));
        }

    }
}