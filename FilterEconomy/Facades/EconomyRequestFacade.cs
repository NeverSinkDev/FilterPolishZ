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
        public Dictionary<string, Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>>> EconomyTierlistOverview { get; set; } = new Dictionary<string, Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>>>();

        public Dictionary<string, ItemList<FilterEconomy.Model.NinjaItem>> PerformRequest(string league, string variation, string branchKey, string prefix, RequestType requestType, string baseStoragePath, string ninjaUrl)
        {
            var economySegmentBranch = FilterPolishConstants.Abbreviations[branchKey];
            var directoryPath = $"{baseStoragePath}/{variation}/{league}/{StringWork.GetDateString()}";
            var fileName = $"{economySegmentBranch}.txt";
            var fileFullPath = $"{directoryPath}/{fileName}";

            string responseString;

            if (!Directory.Exists(directoryPath))
            {   // Check directory
                Directory.CreateDirectory(directoryPath);
            }

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

                    // Store locally
                    Task.Run(() => FileWork.WriteTextAsync(fileFullPath, responseString));
                }
            }
            catch
            {
                throw new Exception("Failed to load economy file: " + branchKey);
            }

            var result = NinjaParser.CreateOverviewDictionary(NinjaParser.ParseNinjaString(responseString).ToList());

            return result;
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
