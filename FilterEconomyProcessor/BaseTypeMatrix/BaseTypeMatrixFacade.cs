using FilterCore;
using FilterCore.Constants;
using FilterCore.Entry;
using FilterCore.FilterComponents.Tier;
using FilterDomain.LineStrategy;
using FilterEconomy.Facades;
using FilterPolishUtil.Extensions;
using FilterPolishUtil.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FilterEconomyProcessor.BaseTypeMatrix
{
    public class BaseTypeMatrixFacade
    {
        private BaseTypeMatrixFacade()
        {

        }

        private static BaseTypeMatrixFacade Instance;

        public static BaseTypeMatrixFacade GetInstance()
        {
            if (Instance == null)
            {
                Instance = new BaseTypeMatrixFacade();
            }

            return Instance;
        }

        private FilterAccessFacade filterAccessFacade;
        public TierListFacade FilterTiers { get; set; }
        public Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>> SortedBaseTypeMatrix { get; private set; }

        public void Initialize(FilterAccessFacade filterAccessFacade)
        {
            this.filterAccessFacade = filterAccessFacade;
            var filter = this.filterAccessFacade.PrimaryFilter;

            // access to all the relevant data in the filter
            this.FilterTiers = TierListFacade.GetInstance();

            // dataMatrix Access
            this.SortedBaseTypeMatrix = BaseTypeDataProvider.BaseTypeTieringMatrixData;
        }

        public void ChangeTier(string section, string itemName, int tierChange)
        {
            var strategy = FilterPolishUtil.FilterPolishConfig.MatrixTiersStrategies[section];

            switch (strategy)
            {
                case FilterPolishUtil.MatrixTieringMode.singleTier:
                    ChangeTier_SingleStrategy(section, itemName, tierChange);
                    break;
                case FilterPolishUtil.MatrixTieringMode.rareTiering:
                    ChangeTier_RareStrategy(section, itemName, tierChange);
                    break;
            }
        }

        public void ChangeTier_SingleStrategy(string section, string itemName, int tierChange)
        {
            if (tierChange == 1)
            {
                RemoveFromBaseTypeTiers(section, itemName);
                AddToSingleBaseTypeTier(section, itemName, tierChange.ToString());
            }
            else
            {
                RemoveFromBaseTypeTiers(section, itemName);
            }
        }

        private void ChangeTier_RareStrategy(string section, string itemName, int tierChange)
        {
            if (tierChange <= 2)
            {
                RemoveFromBaseTypeTiers(section, itemName);
                AddToSingleBaseTypeTier(section, itemName, tierChange.ToString());
            }

            if (tierChange == 3)
            {
                RemoveFromBaseTypeTiers(section, itemName);
                ChangeDropLevel(section, itemName);
            }

            if (tierChange == 4)
            {
                RemoveFromBaseTypeTiers(section, itemName);
            }
        }

        public string LookUpTierName(string section, string itemName)
        {
            var entry = this.LookUpTier(section, itemName);

            if (entry.IsNull())
            {
                return string.Empty;
            }

            return entry.Key ?? string.Empty;
        }

        public void ChangeDropLevel(string section, string itemName, string dropLevel = "")
        {
            if (dropLevel == "")
            {
                var itemStats = BaseTypeDataProvider.BaseTypeData[itemName];
                dropLevel = itemStats["DropLevel"];
            }


            var tier = LookUpTier(section, itemName, true);

            if (tier.IsNull())
            {
                LoggingFacade.LogWarning("No tier found for item!? Fix it future me");
                return;
            }

            var entry = tier.Value.Entry[0];

            if (entry.GetLines("BaseType").Any())
            {
                RemoveFromBaseTypeTiers(section, itemName);
                ChangeDropLevel(itemName, dropLevel);
                return;
            }

            // detect T3 using class/droplevel
            var classLine = entry.Content.GetFirst("Class");
            var dropLine = entry.Content.GetFirst("DropLevel");

            if (classLine != null && dropLine != null)
            {
                (dropLine.Value as NumericValueContainer).Value = dropLevel;
            }
        }

        public void AddToSingleBaseTypeTier(string section, string itemName, string tierlevel)
        {
            string relevantTierName = GetAppropriateTier(section, itemName, tierlevel);

            if (relevantTierName == null)
            {
                LoggingFacade.LogWarning($"Can't find relevant tiername for: {itemName} with tier level {tierlevel}");
            }

            foreach (var tier in this.FilterTiers.TierListData[section].FilterEntries)
            {
                if (tier.Key == relevantTierName)
                {
                    var entry = tier.Value.Entry[0];
                    var values = entry.GetLines("BaseType").First().Value as EnumValueContainer;

                    values.Value.Add(new FilterCore.Line.LineToken()
                    {
                        isQuoted = true,
                        value = itemName
                    });
                }
            }
        }

        public void RemoveFromBaseTypeTiers(string section, string itemName)
        {
            var tier = this.LookUpTier(section, itemName);

            if (tier.Key != null && (tier.Key.Contains("t1") || tier.Key.Contains("t2")))
            {
                var entry = tier.Value.Entry[0];
                var values = entry.GetLines("BaseType").First().Value as EnumValueContainer;
                values.Value.RemoveWhere(x => x.value == itemName);

                if (values.Value.Count == 0)
                {
                    LoggingFacade.LogWarning($"Empty basetype line in: {tier.Key} after removing {itemName}");
                }
            }
        }

        // Gets the appropriate tier from the filter, using itemdata
        private string GetAppropriateTier(string section, string itemName, string tierlevel)
        {
            var strategy = FilterPolishUtil.FilterPolishConfig.MatrixTiersStrategies[section];

            if (strategy != FilterPolishUtil.MatrixTieringMode.rareTiering)
            {
                return "t" + tierlevel;
            }

            var itemStats = BaseTypeDataProvider.BaseTypeData[itemName];
            var itemClass = itemStats["Class"];
            var height = int.Parse(itemStats["Height"]);
            var width = int.Parse(itemStats["Width"]);

            var itemType = "w";
            if (itemClass.Contains("Body") || itemClass.Contains("Glove") || itemClass.Contains("Shield") || itemClass.Contains("Boots") || itemClass.Contains("Helmet"))
            {
                itemType = "a";
            }

            var itemSize = "s";
            if (width > 1 && height > 2)
            {
                itemSize = "l";
            }

            var suffix = "t" + tierlevel + itemType + itemSize;

            foreach (var item in this.FilterTiers.TierListData[section].FilterEntries)
            {
                if (item.Key.Contains(suffix))
                {
                    return item.Key;
                }
            }

            return null;
        }

        public KeyValuePair<string, SingleTier> LookUpTier(string section, string itemName, bool skipLevelCheck = false)
        {
            if (BaseTypeDataProvider.BaseTypeData.ContainsKey(itemName))
            {
                var itemStats = BaseTypeDataProvider.BaseTypeData[itemName];
                var itemClass = itemStats["Class"];
                var dropLevel = itemStats["DropLevel"];

                foreach (var item in this.FilterTiers.TierListData[section].FilterEntries)
                {
                    if (item.Value.Entry.Count == 1)
                    {
                        var entry = item.Value.Entry[0];
                        var baseTypeLine = entry.Content.GetFirst("BaseType");

                        // basetype line
                        if (baseTypeLine != null)
                        {
                            if ((baseTypeLine.Value as EnumValueContainer).Value.Any(x => x.value == itemName))
                            {
                                // entry found
                                return item;
                            }
                        }

                        // detect T3 using class/droplevel
                        var classLine = entry.Content.GetFirst("Class");
                        var dropLine = entry.Content.GetFirst("DropLevel");

                        if (classLine != null && dropLine != null)
                        {
                            if ((classLine.Value as EnumValueContainer).Value.Any(x => x.value == itemClass))
                            {
                                var filterDropLevel = (int.Parse((dropLine.Value as NumericValueContainer).Value));

                                if (!skipLevelCheck)
                                {
                                    if (int.Parse(dropLevel) >= filterDropLevel)
                                    {
                                        return item;
                                    }
                                }
                                else
                                {
                                    return item;
                                }
                            }
                        }

                    }
                    else
                    {
                        throw new Exception("multiple tiers should not be happenning in matrix tiering");
                    }
                }

            }

            return new KeyValuePair<string, SingleTier>(null, null);
        }
    }
}
