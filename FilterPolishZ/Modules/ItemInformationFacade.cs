using FilterCore.Util;
using FilterEconomy.Model.ItemInformationData;
using FilterEconomy.Request.Parsing;
using FilterPolishZ.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FilterPolishZ.Modules.EconomyRequestFacade;

namespace FilterPolishZ.Modules
{
    public class ItemInformationFacade
    {
        public Dictionary<string, Dictionary<string, List<ItemInformationData>>> EconomyTierlistOverview { get; set; } = new Dictionary<string, Dictionary<string, List<ItemInformationData>>>();

        public Dictionary<string, List<ItemInformationData>> LoadItemInformation(string variation, string branchKey)
        {
            var localConfig = LocalConfiguration.GetInstance();
            var baseStoragePath = localConfig.AppSettings["SeedFile Folder"];

            var directoryPath = $"{baseStoragePath}/{variation}";
            var fileName = $"{branchKey}.txt";
            var fileFullPath = $"{directoryPath}/{fileName}";

            string responseString = string.Empty;

            if (!Directory.Exists(directoryPath))
            {   // Check directory
                Directory.CreateDirectory(directoryPath);
            }

            try
            {
                if (File.Exists(fileFullPath))
                {   // Load existing file
                    responseString = FileWork.ReadFromFile(fileFullPath);
                }
            }
            catch
            {
                throw new Exception("Failed to load economy file: " + branchKey);
            }

            return ItemInformationParser.CreateOverviewDictionary(ItemInformationParser.ParseItemInformationString(responseString).ToList());
        }

        public void AddToDictionary(string leagueKey, Dictionary<string, List<ItemInformationData>> dictionary)
        {
            if (!this.EconomyTierlistOverview.ContainsKey(leagueKey))
            {
                this.EconomyTierlistOverview.Add(leagueKey, new Dictionary<string, List<ItemInformationData>>());
            }

            foreach (var keyvalue in dictionary)
            {
                this.EconomyTierlistOverview[leagueKey].Add(keyvalue.Key, keyvalue.Value);
            }
        }
    }
}
