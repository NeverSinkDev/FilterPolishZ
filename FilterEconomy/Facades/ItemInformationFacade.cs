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
using FilterPolishUtil.Collections;
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

        public string LeagueType { get; set; }
        public string BaseStoragePath { get; set; }
        
        public void LoadFromSaveFile()
        {
            foreach (var branchKey in EconomyRequestFacade.GetInstance().EconomyTierlistOverview.Keys)
            {
                var filePath = this.GetItemInfoSaveFilePath(branchKey);

                if (File.Exists(filePath))
                {
                    var fileText = File.ReadAllText(filePath);
                    this.Deserialize(branchKey, fileText);
                }

                var economyData = EconomyRequestFacade.GetInstance();
                this.MigrateAspectDataToEcoData(economyData, branchKey);
            }
        }
        
        public void LoadFromSaveFile(string branchKey)
        {
            var filePath = this.GetItemInfoSaveFilePath(branchKey);

            if (File.Exists(filePath))
            {
                var fileText = File.ReadAllText(filePath);
                this.Deserialize(branchKey, fileText);
            }

            var economyData = EconomyRequestFacade.GetInstance();
            this.MigrateAspectDataToEcoData(economyData, branchKey);
        }

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

        public List<ItemInformationData> this[string tierList,string basetype]
        {
            get
            {
                var targetTierList = this.EconomyTierListOverview[tierList];

                if (!targetTierList.ContainsKey(basetype))
                {
                    return null;
                }

                return targetTierList[basetype];
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

        public void SaveItemInformation(string branchKey)
        {
            var fileFullPath = GetItemInfoSaveFilePath(branchKey);
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

        public Dictionary<string, List<ItemInformationData>> LoadItemInformation(string branchKey)
        {
            var fileFullPath = GetItemInfoSaveFilePath(branchKey);
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
        
        private string GetItemInfoSaveFilePath(string branchKey)
        {
            var directoryPath = $"{this.BaseStoragePath}/{this.LeagueType}";
            var fileName = $"{branchKey}.txt";
            var fileFullPath = $"{directoryPath}/{fileName}";

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return fileFullPath;
        }
        
        public Dictionary<string, ItemList<NinjaItem>> GetItemsThatAreNotInThisList(Dictionary<string, ItemList<NinjaItem>> otherDic, string branchKey, bool isAddingToOtherDic)
        {
            var result = new Dictionary<string, ItemList<NinjaItem>>();

            // add existing ecoData (semi-deep clone)
            if (isAddingToOtherDic)
            {
                otherDic.ToList().ForEach(x =>
                {
                    result.Add(x.Key, new ItemList<NinjaItem>());
                    x.Value.ForEach(y => result[x.Key].Add(y));
                });
            }
            
            // add this.items to result
            foreach (var itemDataPair in this.EconomyTierListOverview[branchKey])
            {
                if (!result.ContainsKey(itemDataPair.Key))
                {
                    result.Add(itemDataPair.Key, new ItemList<NinjaItem>());
                }

                foreach (var item in itemDataPair.Value)
                {
                    if (otherDic[itemDataPair.Key].FirstOrDefault(x => x.Name == item.Name && x.Variant == item.Special) == null)
                    {
                        result[itemDataPair.Key].Add(item.ToNinjaItem());
                    }
                }
            }
            
            return result;
        }
    }
}
