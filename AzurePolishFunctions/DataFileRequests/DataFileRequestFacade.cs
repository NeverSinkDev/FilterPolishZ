using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FilterCore;
using FilterEconomy.Facades;
using FilterEconomy.Model;
using FilterPolishUtil.Collections;
using FilterPolishUtil.Interfaces;
using FilterPolishUtil.Model;

namespace AzurePolishFunctions.DataFileRequests
{
    public class DataFileRequestFacade
    {
        public Dictionary<string, List<string>> FilterStyleSheets { get; set; } = new Dictionary<string, List<string>>();
        public Dictionary<string, string> FilterStyleFilesPaths { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, List<string>> ItemAspects { get; set; } = new Dictionary<string, List<string>>();
        public Dictionary<string, Dictionary<string, string>> BaseTypeData { get; set; }
        public Dictionary<string, Dictionary<string, ItemList<NinjaItem>>> EconomyData { get; set; }
        public List<string> SeedFilter { get; set; }
        public string League { get; set; }

        public void GetAllFiles(string ninjaLeague)
        {
            FilterPolishUtil.Model.LoggingFacade.GetInstance().CustomLoggingAction = x => Console.WriteLine("err: " + x);

            this.League = ninjaLeague;
            
            // Lade Filter Datei -> NS main github -> selective file(s?)
            // Lade Style Dateien -> NS main github -> selective files
            // Lade Aspect Dateien -> https://github.com/NeverSinkDev/Filter-ItemEconomyAspects -> ALL
            this.LoadGitHubFiles();
            
            // Lade BaseType Datei -> FB
            this.BaseTypeData = FilterCore.Constants.BaseTypeDataProvider.BaseTypeData;

            // Lade Economy Dateien -> ninja API
            this.LoadEcoData(ninjaLeague);
            
            var itemData = ItemInformationFacade.GetInstance();
            var economyData = EconomyRequestFacade.GetInstance();
            foreach (var branchKey in economyData.EconomyTierlistOverview.Keys)
            {
                itemData.Deserialize(branchKey, String.Join(Environment.NewLine, this.ItemAspects[branchKey.Replace("->", "")]));
                itemData.MigrateAspectDataToEcoData(economyData, branchKey);
            }
        }

        private void LoadEcoData(string ninjaLeague)
        {
            var result = GenerateFilters.EconomyData;
            var ninjaUrl = "http://poe.ninja/api/Data/"; //Configuration.AppSettings["Ninja Request URL"];
            var variation = ninjaLeague; //Configuration.AppSettings["Ninja League"];
            var league = ""; // Configuration.AppSettings["betrayal"];

            var tasks = new List<Task>();
            foreach (var tuple in FilterPolishUtil.FilterPolishConfig.FileRequestData)
            {
//                PerformEcoRequest(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
                tasks.Add(new Task(() => PerformEcoRequest(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4)));
            }
            tasks.ForEach(x => x.Start());
            Task.WaitAll(tasks.ToArray());

            void PerformEcoRequest(string dictionaryKey, string requestKey, string url, string prefix) =>
                result.AddToDictionary(dictionaryKey,
                    result.PerformRequest(league, variation, requestKey, url, prefix, null, ninjaUrl));

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
                .ForEach(x =>
                {
                    this.FilterStyleSheets.Add(System.IO.Path.GetFileName(x).Split(".").First(), System.IO.File.ReadAllLines(x).ToList());
                    this.FilterStyleFilesPaths.Add(System.IO.Path.GetFileName(x).Split(".").First(), x);
                });
//            FilterPublisher.DeleteDirectory(repoDlPath);

            FilterGenerationConfig.FilterStyles = this.FilterStyleSheets.Keys;
            
            // aspects
            var aspectFolder = new GitHubFileDownloader().Download(nsName, "Filter-ItemEconomyAspects");
            var aspectFiles = System.IO.Directory.EnumerateFiles(aspectFolder).ToList();
            aspectFiles.ForEach(x => this.ItemAspects.Add(System.IO.Path.GetFileName(x).Split(".").First().ToLower(), System.IO.File.ReadAllLines(x).ToList()));
            FilterPublisher.DeleteDirectory(aspectFolder);
        }
    }
}