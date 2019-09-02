using FilterEconomy.Model;
using FilterEconomy.Request;
using FilterEconomy.Request.Parsing;
using FilterPolishUtil;
using FilterPolishUtil.Collections;
using FilterPolishUtil.Constants;
using FilterPolishUtil.Model;
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
            //this.ActiveMetaTags.Add("MetaBiasAspect", DateTime.Now.AddDays(7));
        }

        public static EconomyRequestFacade GetInstance()
        {
            if (instance == null)
            {
                instance = new EconomyRequestFacade();
            }

            return instance;
        }

        private bool didShowNinjaOfflineMessage;

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

                    LoggingFacade.LogInfo($"Loading Economy: Loading Cached File {fileFullPath}");
                    responseString = FileWork.ReadFromFile(fileFullPath);
                }
                else
                {   // Request online file
                    var urlRequest = $"{ninjaUrl}{economySegmentBranch}{prefix}league={variation}";

                    try
                    {
                        responseString = new RestRequest(urlRequest).Execute();
                    }
                    catch (Exception e)
                    {
                        LoggingFacade.LogError($"Loading Economy: Requesting From Ninja {urlRequest}");
                        responseString = null;
                    }
                    
                    // poeNinja down -> use most recent local file
                    if (responseString == null || responseString.Length < 400)
                    {
                        var recentFile = Directory.EnumerateDirectories(directoryPath.Replace(StringWork.GetDateString(), "")).OrderByDescending(Directory.GetCreationTime).FirstOrDefault();

                        if (recentFile != null)
                        {
                            responseString = FileWork.ReadFromFile(recentFile + "/" + fileName);

                            if (responseString != null && responseString.Length >= 400)
                            {
                                if (!didShowNinjaOfflineMessage)
                                {
                                    LoggingFacade.LogWarning("Could not connect to poeNinja. used recent local file instead: " + recentFile + "/" + fileName);
                                    this.didShowNinjaOfflineMessage = true;
                                }
                            }
                        }
                    }

                    // Store locally
                    FileWork.WriteTextAsync(fileFullPath, responseString).Wait(1000);
                }

                if (responseString == null || responseString.Length < 400)
                {
                    LoggingFacade.LogError("poeNinja web request or file content is null/short:\n\n\n" + responseString);
                    responseString = "";
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to load economy file: " + branchKey + ": " + e);
            }

            var result = NinjaParser.CreateOverviewDictionary(NinjaParser.ParseNinjaString(responseString).ToList());

            return result;
        }

        public void EnrichAll(Dictionary<string, List<IDataEnrichment>> enrichments)
        {
            LoggingFacade.LogInfo($"Starting Enriching Economy Information");

            // for every section (divination card etc)
            foreach (var section in this.EconomyTierlistOverview)
            {
                LoggingFacade.LogDebug($"Enriching Economy Information: {section.Key}");
                // go through every item
                foreach (var item in section.Value)
                {
                    enrichments[section.Key].ForEach(z => z.Enrich(item.Key, item.Value));
                }
            }

            LoggingFacade.LogInfo($"Done Enriching Economy Information");
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
