using FilterEconomy.Model.ItemInformationData;
using FilterEconomy.Request.Parsing;
using FilterPolishUtil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using FilterEconomy.Model;
using FilterEconomy.Model.ItemAspects;
using Newtonsoft.Json;

namespace FilterEconomy.Facades
{
    public class ItemInformationFacade
    {
        private ItemInformationFacade() { }

        private static ItemInformationFacade instance;

        public static ItemInformationFacade GetInstance()
        {
            if (instance == null)
            {
                instance = new ItemInformationFacade();
            }

            return instance;
        }

        public Dictionary<string, Dictionary<string, List<ItemInformationData>>> EconomyTierlistOverview { get; set; } = new Dictionary<string, Dictionary<string, List<ItemInformationData>>>();
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings { Converters = new List<JsonConverter> { new ItemAspectFactory() } };

        public void ExtractAspectDataFromEcoData(EconomyRequestFacade ecoData, string branchKey)
        {
            var otherDic = this.EconomyTierlistOverview[branchKey];
            
            foreach (var keyValuePair in ecoData.EconomyTierlistOverview[branchKey])
            {
                var baseType = keyValuePair.Key;
                var itemList = keyValuePair.Value;

                if (!otherDic.ContainsKey(baseType))
                {
                    otherDic.Add(baseType, new List<ItemInformationData>());
                }
                var otherItems = otherDic[baseType];

                foreach (var item in itemList)
                {
                    var other = otherItems.FirstOrDefault(x => x.Name == item.Name);
                    if (other == null)
                    {
                        otherItems.Add(new ItemInformationData
                        {
                            Name = item.Name,
                            BaseType = item.BaseType
                            // todo: special?
                        });
                        other = otherItems.First(x => x.Name == item.Name);
                    }

                    other.Aspects = new List<IItemAspect>(item.Aspects);
                }
            }
        }

        public void MigrateAspectDataToEcoData(EconomyRequestFacade ecoData, string branchKey)
        {
            var otherDic = ecoData.EconomyTierlistOverview[branchKey];
            
            foreach (var keyValuePair in this.EconomyTierlistOverview[branchKey])
            {
                var baseType = keyValuePair.Key;
                var itemList = keyValuePair.Value;
                
                if (!otherDic.ContainsKey(baseType)) throw new Exception("unknown base"); // todo
                var otherItems = otherDic[baseType];

                foreach (var item in itemList)
                {
                    var other = otherItems.FirstOrDefault(x => x.Name == item.Name);
                    if (other == null) throw new Exception("unknown item/unique"); // todo

                    other.Aspects = new ObservableCollection<IItemAspect>(item.Aspects);
                }
            }
        }

        public void SaveItemInformation(string leagueType, string branchKey, string baseStoragePath)
        {
            var fileFullPath = this.GetItemInfoSaveFilePath(leagueType, branchKey, baseStoragePath);
            this.SaveItemInformation(fileFullPath, branchKey);
        }

        public void SaveItemInformation(string filePath, string branchKey)
        {
            FileWork.WriteTextAsync(filePath, this.Serialize(branchKey));
        }
        
        public string GetItemInfoSaveFilePath(string leagueType, string branchKey, string baseStoragePath)
        {
            var directoryPath = $"{baseStoragePath}/{leagueType}";
            var fileName = $"{branchKey}.txt";
            var fileFullPath = $"{directoryPath}/{fileName}";

            if (!Directory.Exists(directoryPath))
            {   // Check directory
                Directory.CreateDirectory(directoryPath);
            }

            return fileFullPath;
        }

        public string Serialize(string branchKey)
        {
            return JsonConvert.SerializeObject(this.EconomyTierlistOverview[branchKey], JsonSettings);
        }

        public void Deserialize(string branchKey, string input)
        {
            var newObj = new Dictionary<string, List<ItemInformationData>>();
            JsonConvert.PopulateObject(input, newObj, JsonSettings);
            this.EconomyTierlistOverview[branchKey] = newObj;
        }

        public Dictionary<string, List<ItemInformationData>> LoadItemInformation(string leagueType, string branchKey, string baseStoragePath)
        {
            var fileFullPath = this.GetItemInfoSaveFilePath(leagueType, branchKey, baseStoragePath);
            string responseString = string.Empty;

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
