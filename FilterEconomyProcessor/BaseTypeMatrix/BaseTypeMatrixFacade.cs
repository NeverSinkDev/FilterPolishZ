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
using System.Text;

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
        public TierGroup FilterTiers {get; set;}
        public Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>> SortedBaseTypeMatrix { get; private set; }

        public void Initialize(FilterAccessFacade filterAccessFacade)
        {
            this.filterAccessFacade = filterAccessFacade;
            var filter = this.filterAccessFacade.PrimaryFilter;

            // access to all the relevant data in the filter
            this.FilterTiers = TierListFacade.GetInstance().TierListData["rr"];

            // dataMatrix Access
            this.SortedBaseTypeMatrix = BaseTypeDataProvider.BaseTypeTieringMatrixData;
        }

        public void ChangeTier(string itemName, int tierChange)
        {
            if (tierChange <= 2)
            {
                RemoveFromBaseTypeTiers(itemName);
                AddToSingleBaseTypeTier(itemName, tierChange.ToString());
            }

            if (tierChange == 3)
            {
                RemoveFromBaseTypeTiers(itemName);
                ChangeDropLevel(itemName);
            }

            if (tierChange == 4)
            {
                RemoveFromBaseTypeTiers(itemName);
            }
        }

        public string LookUpTierName(string itemName)
        {
            var entry = this.LookUpTier(itemName);

            if (entry.IsNull())
            {
                return string.Empty;
            }

            return entry.Key ?? string.Empty;
        }

        public void ChangeDropLevel(string itemName, string dropLevel = "")
        {
            if (dropLevel == "")
            {
                var itemStats = BaseTypeDataProvider.BaseTypeData[itemName];
                dropLevel = itemStats["DropLevel"];
            }


            var tier = LookUpTier(itemName);
            var entry = tier.Value.Entry[0];
            
            if (entry.GetLines("BaseType").Any())
            {
                RemoveFromBaseTypeTiers(itemName);
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

        public void AddToSingleBaseTypeTier(string itemName, string tierlevel)
        {
            string relevantTierName = GetAppropriateTier(itemName, tierlevel);

            if (relevantTierName == null)
            {
                LoggingFacade.LogWarning($"Can't find relevant tiername for: {itemName} with tier level {tierlevel}");
            }

            foreach (var tier in this.FilterTiers.FilterEntries)
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

        public void RemoveFromBaseTypeTiers(string itemName)
        {
            var tier = this.LookUpTier(itemName);

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

        private string GetAppropriateTier(string itemName, string tierlevel)
        {
            foreach (var item in this.FilterTiers.FilterEntries)
            {
                 if (item.Key.Contains(tierlevel))
                {
                    return item.Key;
                }
            }

            return null;
        }

        public KeyValuePair<string,SingleTier> LookUpTier(string itemName)
        {
            if (BaseTypeDataProvider.BaseTypeData.ContainsKey(itemName))
            {
                var itemStats = BaseTypeDataProvider.BaseTypeData[itemName];
                var itemClass = itemStats["Class"];
                var dropLevel = itemStats["DropLevel"];

                foreach (var item in this.FilterTiers.FilterEntries)
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

                                if (int.Parse(dropLevel) >= filterDropLevel)
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
