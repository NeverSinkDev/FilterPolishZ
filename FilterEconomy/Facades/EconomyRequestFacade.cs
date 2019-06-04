using FilterEconomy.Request;
using FilterEconomy.Request.Parsing;
using FilterPolishUtil;
using FilterPolishUtil.Collections;
using FilterPolishUtil.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FilterEconomy.Facades
{
    public class EconomyRequestFacade
    {
        private EconomyRequestFacade()
        {
            this.ActiveMetaTags.Add("MetaBiasAspect", DateTime.Now.AddDays(7));
        }

        public static EconomyRequestFacade GetInstance()
        {
            if (instance == null)
            {
                instance = new EconomyRequestFacade();
            }

            return instance;
        }

        private static EconomyRequestFacade instance;

        public Dictionary<string, Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>>> EconomyTierlistOverview { get; set; } = new Dictionary<string, Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>>>();

        public Dictionary<string, DateTime> ActiveMetaTags { get; set; } = new Dictionary<string, DateTime>();

        public Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>> PerformRequest(string league, string variation, string branchKey, string url, string prefix, RequestType requestType, string baseStoragePath, string ninjaUrl)
        {
            var economySegmentBranch = url;
            var directoryPath = $"{baseStoragePath}/{variation}/{league}/{StringWork.GetDateString()}";
            var fileName = $"{branchKey}.txt";
            var fileFullPath = $"{directoryPath}/{fileName}";

            string responseString;

            try
            {
                if (File.Exists(fileFullPath) && requestType != RequestType.ForceOnline)
                {   // Load existing file
                    responseString = FileWork.ReadFromFile(fileFullPath);
                }
                else
                {   // Request online file
                    var urlRequest = $"{ninjaUrl}{economySegmentBranch}{prefix}league={variation}";
                    responseString = new RestRequest(urlRequest).Execute();
                    
                    // poeNinja down -> use most recent local file
                    if (responseString == null || responseString.Length < 400)
                    {
                        var recentFile = Directory.EnumerateDirectories(directoryPath.Replace(StringWork.GetDateString(), "")).OrderByDescending(Directory.GetCreationTime).FirstOrDefault();

                        if (recentFile != null)
                        {
                            responseString = FileWork.ReadFromFile(recentFile + "/" + fileName);

                            if (responseString != null && responseString.Length >= 400)
                            {
                                InfoPopUpMessageDisplay.ShowInfoMessageBox("Could not connect to poeNinja. used recent local file instead: " + recentFile + "/" + fileName);
                            }
                        }
                    }

                    // Store locally
                    Task.Run(() => FileWork.WriteTextAsync(fileFullPath, responseString));
                }

                if (responseString == null || responseString.Length < 400)
                {
                    InfoPopUpMessageDisplay.ShowError("poeNinja web request or file content is null/short:\n\n\n" + responseString);
                    responseString = "";
                }
            }
            catch
            {
                throw new Exception("Failed to load economy file: " + branchKey);
            }

            var result = NinjaParser.CreateOverviewDictionary(NinjaParser.ParseNinjaString(responseString).ToList());

            return result;
        }

        public void EnrichAll()
        {
            // for every section (divination card etc)
            foreach (var section in this.EconomyTierlistOverview)
            {
                // go through every item
                foreach (var item in section.Value)
                {
                    EnrichmentProcedureConfiguration.EnrichmentProcedures[section.Key].ForEach(z => z.Enrich(item.Key, item.Value));
                }
            }
        }

        public void Reset()
        {
            this.EconomyTierlistOverview.Clear();
            this.ActiveMetaTags.Clear();
        }

        public void AddToDictionary(string leagueKey, Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>> dictionary)
        {
            if (!this.EconomyTierlistOverview.ContainsKey(leagueKey))
            {
                this.EconomyTierlistOverview.Add(leagueKey, new Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>>());
            }

            foreach (var keyvalue in dictionary)
            {
                this.EconomyTierlistOverview[leagueKey].Add(keyvalue.Key, keyvalue.Value);
            }
        }

        public enum RequestType
        {
            Dynamic,
            ForceOnline
        }
    }
}
