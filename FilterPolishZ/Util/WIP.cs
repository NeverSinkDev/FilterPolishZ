using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishZ.Util
{


    //----------

    //            return this.InitialList.Where(x => Selectors.All(func => func(x)))
    //                .GroupBy(x => x.BaseType)
    //                .ToDictionary(group => group.Key, group => group.OrderBy(x => x.LevelRequired).ToList());

    //				----------

    //TLM.pricedBasesOverview.AddSelector(x => x.Variant?.Equals(variant, StringComparison.CurrentCultureIgnoreCase) ?? (variant == null));
    //    var dictionary = TLM.pricedBasesOverview.InitializeStructure();
    //    var processedData = TLM.pricedBasesOverview.ProcessBaseTypeData(dictionary, 2, 8, 10);

    //            foreach (var item in processedData)
    //            {
    //        var type = item.Value.Select(z => z.Class).First().ToString();
    //        if (!classBasedPricedItems.ContainsKey(type))
    //        {
    //            classBasedPricedItems.Add(type, new Dictionary<string, List<NinjaItem>>());
    //        }
    //        classBasedPricedItems[type].Add(item.Key, item.Value);
    //    }

    //    classBasedPricedItems.ToString();

    //    List<string> results = new List<string>();

    //            foreach (var item in classBasedPricedItems)
    //            {
    //                results.Add("#####");
    //                results.Add(item.Key.ToUpper());
    //                results.Add("");

    //                var values = item.Value.Values;
    //    var orderedList = values.OrderBy(x => x.Sum(z => z.CVal * z.IndexedCount) / x.Sum(j => j.IndexedCount)).ToList();
    //    orderedList.ForEach(x => results.Add($"{x.Select(z => z.BaseType).First().PadRight(32,' ')} {FormatPriceString(x)}"));

    //                results.Add("");
    //                results.Add(orderedList.Average(x => x.Sum(z => z.CVal* z.IndexedCount) / x.Sum(j => j.IndexedCount)).ToString());
    //            }




    //var T2tresh = 8;
    //var T1tresh = 28;

    //var T1on86 = new List<NinjaItem>();
    //var T1on82 = new List<NinjaItem>();
    //var T1on83 = new List<NinjaItem>();
    //var T1on84 = new List<NinjaItem>();
    //var T1on85 = new List<NinjaItem>();
    //var T2on82 = new List<NinjaItem>();
    //var T2on86 = new List<NinjaItem>();

    //var dictionaries = new List<Dictionary<string, NinjaItem>>();



    //            foreach (var item in processedData)
    //            {
    //                Func<int, NinjaItem> atValue = (int val) => (item.Value?.FirstOrDefault(x => x.LevelRequired == val));

    //var val82 = atValue(82)?.CVal ?? 0;
    //var val86 = atValue(86)?.CVal ?? 0;
    //var val84 = atValue(84)?.CVal ?? 0;
    //var val85 = atValue(84)?.CVal ?? 0;
    //var valAvg = item.Value.Sum(z => z.CVal * z.IndexedCount) / item.Value.Sum(j => j.IndexedCount);
    //bool got86 = false;

    //                if (val82 > T1tresh && ((val86 == 0 ? val85 : val86) > T1tresh))
    //                {
    //                    T1on82.Add(atValue(82));
    //                    T2on82.Add(atValue(82));
    //                    continue;
    //                }
    //                else if (val84 > T1tresh && (val82 > T2tresh || val86 > T1tresh))
    //                {
    //                    T1on84.Add(atValue(84));
    //                    T2on82.Add(atValue(82));
    //                    continue;
    //                }
    //                else if (val86 > T1tresh && (val82 > T2tresh || val85 > T2tresh))
    //                {
    //                    T1on86.Add(atValue(86));
    //                    got86 = true;
    //                }

    //                if (val82 > T2tresh)
    //                {
    //                    T2on82.Add(atValue(82));
    //                }
    //                else if (val86 > T2tresh && !got86)
    //                {
    //                    T2on86.Add(atValue(86));
    //                }
    //            }

    //            results.Add("T1 at 82:");
    //            results.Add(string.Join(" ", T1on82.Select(x => $"\"{x.BaseType}\"").ToList()));
    //            results.Add("");

    //            results.Add("T1 at 84:");
    //            results.Add(string.Join(" ", T1on84.Select(x => $"\"{x.BaseType}\"").ToList()));
    //            results.Add("");

    //            results.Add("T1 at 86:");
    //            results.Add(string.Join(" ", T1on86.Select(x => $"\"{x.BaseType}\"").ToList()));
    //            results.Add("");

    //            results.Add("T2 at 82:");
    //            results.Add(string.Join(" ", T2on82.Select(x => $"\"{x.BaseType}\"").ToList()));
    //            results.Add("");

    //            results.Add("T2 at 86:");
    //            results.Add(string.Join(" ", T2on86.Select(x => $"\"{x.BaseType}\"").ToList()));
    //            results.Add("");


    //            var stringifiedResult = string.Join("\r\n", results);
    //System.IO.File.WriteAllText(Util.GetRootPath() + $"/{variant ?? "normal"}.txt", stringifiedResult);
    //        }

    //        private static string FormatPriceString(List<NinjaItem> x)
    //{
    //    string result = "";
    //    result += Math.Round(x.Sum(z => z.CVal * z.IndexedCount) / x.Sum(j => j.IndexedCount)).ToString().PadRight(8);

    //    for (int i = 82; i <= 86; i++)
    //    {
    //        var item = x.Where(z => z.LevelRequired == i).FirstOrDefault();
    //        result += item != null ? $"{Math.Round(item.CVal).ToString().PadRight(3)}({item.IndexedCount})".PadRight(10) : "".PadRight(10);
    //    }
    //    result += x.Sum(z => z.IndexedCount);

    //    return result;

    //public List<KeyValuePair<string, List<NinjaItem>>> ProcessBaseTypeData(Dictionary<string, List<NinjaItem>> input, int minAvgChaosValue, int minMaxChaosValue, int minIndexedQuantity)
    //{
    //    // Remove items that are way too cheap to be bothered with (note ilvl 86 tier is very expensive, so we gotta start high)
    //    //var adjustedPrices = input.Where(x => x.Value.Sum(z => z.CVal * z.IndexedCount) / x.Value.Sum(j => j.IndexedCount) > minAvgChaosValue).ToList();
    //    //adjustedPrices.ForEach(x =>
    //    //{
    //    //    if (x.Value.Select(z => z.IndexedCount).Sum() < 25)
    //    //    {
    //    //        var max = x.Value.Max(c => c.CVal);
    //    //        var avg = x.Value.Where(c => c.CVal != max).Sum(z => z.CVal * z.IndexedCount) / x.Value.Sum(j => j.IndexedCount);
    //    //        x.Value.RemoveAll(z => z.CVal > avg * 50);
    //    //    }
    //    //});

    //    //var solidMinPrices = input.Where(x => x.Value.Min(z => z.CVal > minAvgChaosValue)).ToList();
    //    var solidMinPrices = input.Where(x => x.Value.Sum(z => z.CVal * z.IndexedCount) / x.Value.Sum(j => j.IndexedCount) >= minAvgChaosValue).ToList();
    //    var solidMaxPrices = solidMinPrices.Where(x => x.Value.Max(z => z.CVal) > minMaxChaosValue).ToList();

    //    // Remove extreme anomalies -> usually super low level items
    //    //var normalBases = solidMaxPrices.Where(x => x.Value.Sum(z => z.IndexedCount) < minIndexedQuantity).ToList();

    //    // Remove weird cases where high level items are not quite as expensive as lower tier ones (random offers)
    //    var removedAnomalies = solidMaxPrices
    //        .Where(x => (x.Value.Sum(z => z.CVal * z.IndexedCount) / x.Value.Sum(j => j.IndexedCount)) >= minMaxChaosValue ||
    //        x.Value.Sum(j => j.IndexedCount) >= minIndexedQuantity * 4).ToList();

    //    var relevantCount = removedAnomalies.Where(x => x.Value.Select(y => y.IndexedCount).Sum() >= minIndexedQuantity).ToList();

    //    return relevantCount;
    //}
}
