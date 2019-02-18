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
        // singleton
        private ItemInformationFacade() { }
        private static ItemInformationFacade instance;
        public static ItemInformationFacade GetInstance() => instance ?? (instance = new ItemInformationFacade());

        public Dictionary<string, Dictionary<string, List<ItemInformationData>>> EconomyTierListOverview { get; set; } = new Dictionary<string, Dictionary<string, List<ItemInformationData>>>();
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings { Converters = new List<JsonConverter> { new ItemAspectFactory() } };

        public void ExtractAspectDataFromEcoData(EconomyRequestFacade ecoData, string branchKey)
        {
            var targetDic = this.EconomyTierListOverview[branchKey];
            
            foreach (var keyValuePair in ecoData.EconomyTierlistOverview[branchKey])
            {
                var baseType = keyValuePair.Key;
                var sourceItemList = keyValuePair.Value;

                if (!targetDic.ContainsKey(baseType))
                {
                    targetDic.Add(baseType, new List<ItemInformationData>());
                }
                
                var targetItems = targetDic[baseType];

                foreach (var sourceItem in sourceItemList)
                {
                    var targetItem = targetItems.FirstOrDefault(x => x.Name == sourceItem.Name);
                    if (targetItem == null)
                    {
                        targetItem = new ItemInformationData
                        {
                            Name = sourceItem.Name,
                            BaseType = sourceItem.BaseType,
                            Special = sourceItem.Variant
                        };
                        
                        targetItems.Add(targetItem);
                    }

                    targetItem.Aspects = new List<IItemAspect>(sourceItem.Aspects);
                }
            }
        }

        public void MigrateAspectDataToEcoData(EconomyRequestFacade ecoData, string branchKey)
        {
            var targetDic = ecoData.EconomyTierlistOverview[branchKey];
            
            foreach (var keyValuePair in this.EconomyTierListOverview[branchKey])
            {
                var baseType = keyValuePair.Key;
                var sourceItemList = keyValuePair.Value;
                
                if (!targetDic.ContainsKey(baseType)) throw new Exception("unknown base"); // todo
                var targetItems = targetDic[baseType];

                foreach (var sourceItem in sourceItemList)
                {
                    var targetItem = targetItems.FirstOrDefault(x => x.Name == sourceItem.Name);
                    if (targetItem == null) throw new Exception("unknown item/unique"); // todo

                    targetItem.Aspects = new ObservableCollection<IItemAspect>(sourceItem.Aspects);
                }
            }
        }

        public void SaveItemInformation(string leagueType, string branchKey, string baseStoragePath)
        {
            var fileFullPath = GetItemInfoSaveFilePath(leagueType, branchKey, baseStoragePath);
            this.SaveItemInformation(fileFullPath, branchKey);
        }

        public void SaveItemInformation(string filePath, string branchKey)
        {
            FileWork.WriteTextAsync(filePath, this.Serialize(branchKey));
        }

        public string Serialize(string branchKey)
        {
            return JsonConvert.SerializeObject(this.EconomyTierListOverview[branchKey], JsonSettings);
        }

        public void Deserialize(string branchKey, string input)
        {
            var newObj = new Dictionary<string, List<ItemInformationData>>();
            JsonConvert.PopulateObject(input, newObj, JsonSettings);
            this.EconomyTierListOverview[branchKey] = newObj;
        }

        public Dictionary<string, List<ItemInformationData>> LoadItemInformation(string leagueType, string branchKey, string baseStoragePath)
        {
            var fileFullPath = GetItemInfoSaveFilePath(leagueType, branchKey, baseStoragePath);
            var responseString = File.Exists(fileFullPath) ? FileWork.ReadFromFile(fileFullPath) : "";
            return ItemInformationParser.CreateOverviewDictionary(ItemInformationParser.ParseItemInformationString(responseString).ToList());
        }

        public void AddToDictionary(string leagueKey, Dictionary<string, List<ItemInformationData>> dictionary)
        {
            if (!this.EconomyTierListOverview.ContainsKey(leagueKey))
            {
                this.EconomyTierListOverview.Add(leagueKey, new Dictionary<string, List<ItemInformationData>>());
            }

            foreach (var keyValuePair in dictionary)
            {
                this.EconomyTierListOverview[leagueKey].Add(keyValuePair.Key, keyValuePair.Value);
            }
        }
        
        public static string GetItemInfoSaveFilePath(string leagueType, string branchKey, string baseStoragePath)
        {
            var directoryPath = $"{baseStoragePath}/{leagueType}";
            var fileName = $"{branchKey}.txt";
            var fileFullPath = $"{directoryPath}/{fileName}";

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return fileFullPath;
        }
    }
}
