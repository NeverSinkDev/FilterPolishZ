using FilterCore.Constants;
using FilterEconomy.Facades;
using FilterPolishUtil;
using FilterPolishUtil.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilterEconomyProcessor.ClassAbstraction
{
    public class InfluencedBasesAbstractions
    {
        public void Execute()
        {

            LoggingFacade.LogInfo($"Performing Influenced Class Abstractions");

            List<KeyValuePair<float, string>> sortedConfList = new List<KeyValuePair<float, string>>();
            List<KeyValuePair<float, string>> sortedPriceList = new List<KeyValuePair<float, string>>();

            var itemLevel = 85;
            foreach (var section in EconomyRequestFacade.GetInstance().EconomyTierlistOverview)
            {
                if (!section.Key.Contains("rare"))
                {
                    continue;
                }

                var confList = new Dictionary<string, List<float>>();
                var valueList = new Dictionary<string, List<float>>();

                // Iterate through all items
                foreach (var item in section.Value)
                {
                    if (BaseTypeDataProvider.BaseTypeData.ContainsKey(item.Key))
                    {
                        var itemInfo = BaseTypeDataProvider.BaseTypeData[item.Key];
                        int dropLevel = int.Parse(itemInfo["DropLevel"]);
                        string itemClass = itemInfo["Class"].ToLower();

                        if (FilterPolishConfig.SpecialBases.Contains(item.Key))
                        {
                            continue;
                        }

                        if (!valueList.ContainsKey(itemClass))
                        {
                            valueList[itemClass] = new List<float>();
                            confList[itemClass] = new List<float>();
                        }

                        // valueList[itemClass].Add(item.Value.ValueMultiplier * item.Value.GetPrice(itemLevel));

                        if (item.Value.ValueMultiplier > 0.4f)
                        {

                            valueList[itemClass].Add(item.Value.GetPrice(itemLevel) * item.Value.ValueMultiplier);
                        }
                        else
                        {
                            // valueList[itemClass].Add(0);
                        }
                    }
                }

                // Sort each list and omit the outliers
                var filteredValueList = new Dictionary<string, List<float>>();

                foreach (var item in valueList)
                {
                    filteredValueList.Add(item.Key,
                        item.Value
                        .OrderBy(x => x).ToList());
                    //.RemoveFrom(2));

                }

                foreach (var item in filteredValueList)
                {
                    if (item.Value.Count == 0)
                    {
                        continue;
                    }

                    var conf = item.Value.Average();
                    sortedPriceList.Add(new KeyValuePair<float, string>(conf, $"[{itemLevel}]{section.Key} >> {item.Key} >> { conf }"));
                }
            }

            var sortedValueList1 = sortedPriceList.OrderBy(x => x.Key).ToList();

            foreach (var item in sortedValueList1)
            {
                LoggingFacade.LogDebug(item.Value);
            }
        }
    }
}
