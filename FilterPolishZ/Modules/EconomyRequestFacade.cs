using FilterCore.Util;
using FilterEconomy.Request;
using FilterEconomy.Request.Parsing;
using FilterPolishZ.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishZ.Modules
{
    public class EconomyRequestFacade
    {
        public Dictionary<string, Dictionary<string, List<FilterEconomy.Model.NinjaItem>>> EconomyTierlistOverview { get; set; } = new Dictionary<string, Dictionary<string, List<FilterEconomy.Model.NinjaItem>>>();

        public Dictionary<string, List<FilterEconomy.Model.NinjaItem>> PerformRequest(string league, string variation, string branchKey, string prefix, RequestType requestType)
        {
            var localConfig = LocalConfiguration.GetInstance();

            var baseStoragePath = localConfig.AppSettings["SeedFile Folder"];
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
                    var urlRequest = $"{localConfig.AppSettings["Ninja Request URL"]}{economySegmentBranch}{prefix}league={variation}";
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

        public void AddToDictionary(string leagueKey, Dictionary<string, List<FilterEconomy.Model.NinjaItem>> dictionary)
        {
            if (!this.EconomyTierlistOverview.ContainsKey(leagueKey))
            {
                this.EconomyTierlistOverview.Add(leagueKey, new Dictionary<string, List<FilterEconomy.Model.NinjaItem>>());
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
