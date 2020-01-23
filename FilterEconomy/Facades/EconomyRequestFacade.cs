using FilterEconomy.Model;
using FilterEconomy.Request;
using FilterEconomy.Request.Parsing;
using FilterPolishUtil;
using FilterPolishUtil.Collections;
using FilterPolishUtil.Interfaces;
using FilterPolishUtil.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FilterPolishUtil.Extensions;
using FilterCore.Constants;

namespace FilterEconomy.Facades
{
    public class EconomyRequestFacade : ICleanable
    {
        private EconomyRequestFacade()
        {
            // this.ActiveMetaTags.Add("EarlyLeagueInterestAspect", DateTime.Now.AddDays(7));
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

        public Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>> PerformRequest(string league, string leagueType, string branchKey, string url, string baseStoragePath)
        {
            var economySegmentBranch = url;
            var directoryPath = $"{baseStoragePath}/{leagueType}/{league}/{StringWork.GetDateString()}";
            var fileName = $"{branchKey}.txt";
            var fileFullPath = $"{directoryPath}/{fileName}";

            string responseString;

            try
            {
                if (FilterPolishConfig.ActiveRequestMode != RequestType.ForceOnline && File.Exists(fileFullPath))
                {   // Load existing file

                    LoggingFacade.LogInfo($"Loading Economy: Loading Cached File {fileFullPath}");
                    responseString = FileWork.ReadFromFile(fileFullPath);
                }
                else
                {   // Request online file
                    string variation = this.CreateNinjaLeagueParameter(league, leagueType);

                    var urlRequest = $"{economySegmentBranch}&league={variation}";

                    try
                    {
                        responseString = new RestRequest(urlRequest).Execute();
                    }
                    catch (Exception)
                    {
                        LoggingFacade.LogError($"Loading Economy: Requesting From Ninja {urlRequest}");
                        responseString = null;
                    }
                    
                    // poeNinja down -> use most recent local file
                    if ((responseString == null || responseString.Length < 400) && FilterPolishConfig.ActiveRequestMode == RequestType.Dynamic)
                    {
                        var recentFile = Directory
                            .EnumerateDirectories(directoryPath.Replace(StringWork.GetDateString(), ""))
                            .Where(x => File.Exists(x + "/" + fileName))
                            .OrderByDescending(Directory.GetCreationTime)
                            .FirstOrDefault();

                        if (recentFile != null && File.Exists(recentFile + "/" + fileName))
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
                        else
                        {
                            throw new Exception("did not find any old ninja files");
                        }
                    }

                    if (!string.IsNullOrEmpty(responseString) && FilterPolishConfig.ActiveRequestMode == RequestType.Dynamic)
                    {
                        // Store locally
                        FileWork.WriteText(fileFullPath, responseString);
                    }
                }

                if (responseString == null || responseString.Length < 400)
                {
                    LoggingFacade.LogError("poeNinja web request or file content is null/short:\n\n\n" + responseString);
                    throw new Exception("poeNinja web request or file content is null/short:\n\n\n" + responseString);
                }
            }
            catch (Exception e)
            {
                LoggingFacade.LogError("Failed to load economy file: " + branchKey + ": " + e);
                return null;
            }

            var result = NinjaParser.CreateOverviewDictionary(NinjaParser.ParseNinjaString(responseString, branchKey).ToList());

            return result;
        }

        public void CreateSubEconomyTiers()
        {
            LoggingFacade.LogInfo($"Generating Sub-Economy Tiers");

            List<string> influenceTypes = new List<string>() { "Shaper", "Elder", "Warlord", "Crusader", "Redeemer", "Hunter" };
            var metaDictionary = new Dictionary<string, Dictionary<string, ItemList<NinjaItem>>>();
            var otherbases = new Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>>();

            influenceTypes.ForEach(x => metaDictionary.Add(x, new Dictionary<string, ItemList<NinjaItem>>()));

            foreach (var items in this.EconomyTierlistOverview["basetypes"])
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

            this.AddToDictionary("rare->shaper", metaDictionary["Shaper"]);
            this.AddToDictionary("rare->elder", metaDictionary["Elder"]);
            this.AddToDictionary("rare->warlord", metaDictionary["Warlord"]);
            this.AddToDictionary("rare->crusader", metaDictionary["Crusader"]);
            this.AddToDictionary("rare->redeemer", metaDictionary["Redeemer"]);
            this.AddToDictionary("rare->hunter", metaDictionary["Hunter"]);
            this.AddToDictionary("generalcrafting", otherbases);

            LoggingFacade.LogInfo($"Done Generating Sub-Economy Tiers");
        }

        private string CreateNinjaLeagueParameter(string league, string leagueType)
        {
            if (leagueType.ToLower() == "hardcore" && league.ToLower() == "standard")
            {
                return "Hardcore";
            }

            if (leagueType.ToLower() == "standard" || league.ToLower() == "standard")
            {
                return "Standard";
            }

            if (leagueType.ToLower() == "hardcore" || leagueType.ToLower() == "tmphardcore")
            {
                return "Hardcore " + league;
            }

            if (league == string.Empty)
            {
                LoggingFacade.LogWarning("League information missing! Using Standard!!!");
                return "Standard";
            }

            return league;
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
                    if (!enrichments.ContainsKey(section.Key))
                    {
                        continue;
                    }

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

        public void Clean()
        {
            this.Reset();
            instance = null;
        }
    }
}
