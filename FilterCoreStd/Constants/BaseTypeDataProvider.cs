using FilterPolishUtil.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;

namespace FilterCore.Constants
{
    public static class BaseTypeDataProvider
    {
        private const string DataFileUrl = "https://www.filterblade.xyz/datafiles/other/BasetypeStorage.csv";

        private static Dictionary<string, Dictionary<string, string>> data;
        private static Dictionary<string, List<Dictionary<string, string>>> classBasedTierList;

        public static Dictionary<string, Dictionary<string, string>> BaseTypeData => data ?? (data = LoadData());
        public static Dictionary<string, List<Dictionary<string, string>>> ClassBasedTierList = classBasedTierList ?? (classBasedTierList = CreateClassBasedList());

        private static Dictionary<string, Dictionary<string, string>> LoadData()
        {
            // this client somehow stopped working. on one hand it suddenly needed a user agent specified, on the other it
            // crashed because of endless redirects.
            //            var client = new WebClient();
            //            client.Headers.Add("user-agent", "any");  
            //            var fullString = client.DownloadString(DataFileUrl);

            var client = (HttpWebRequest)WebRequest.Create(DataFileUrl);
            client.UserAgent = "any";
            client.CookieContainer = new CookieContainer();

            var fullString = "";
            var reader = new StreamReader(client.GetResponse().GetResponseStream());
            while (!reader.EndOfStream)
            {
                fullString += (reader.ReadLine()) + "\n";
            }

            string[] stats = null;
            var baseTypeData = new Dictionary<string, Dictionary<string, string>>();

            var isFirstLine = true;
            foreach (var line in fullString.Split('\n'))
            {
                if (string.IsNullOrEmpty(line)) continue;

                var words = line.Split(',');

                if (isFirstLine)
                {
                    stats = words;
                    isFirstLine = false;
                    continue;
                }

                string baseType = null;
                var statDic = new Dictionary<string, string>();
                for (var i = 0; i < words.Length; i++)
                {
                    var value = words[i];
                    var stat = stats[i];

                    if (stat == "BaseType") baseType = value;

                    statDic.Add(stat, value);
                }

                baseTypeData.Add(baseType, statDic);
            }

            return baseTypeData;
        }

        private static Dictionary<string, List<Dictionary<string, string>>> CreateClassBasedList()
        {
            Dictionary<string, List<Dictionary<string, string>>> results = new Dictionary<string, List<Dictionary<string, string>>>();

            // transform a basetype:info list to -> class:(basetype:info)

            foreach (var item in BaseTypeData)
            {
                var iClass = item.Value["Class"];
                if (!results.ContainsKey(iClass))
                {
                    results.Add(iClass, new List<Dictionary<string, string>>());
                }

                results[iClass].Add(item.Value);
            }

            // Order lists by itemlevel
            var keys = new List<string>();
            foreach (var item in results)
            {
                keys.Add(item.Key);
            }

            EnrichItemsWithLevelSorting(results, keys);
            EnrichItemsWithApsSorting(results);

            return results;
        }

        private static void EnrichItemsWithApsSorting(Dictionary<string, List<Dictionary<string, string>>> results)
        {
            foreach (var item in results)
            {
                double classCount = results[item.Key].Count;
                var reducedList = results[item.Key].Select(x => x["Game:APS"]).Where(x => !string.IsNullOrEmpty(x)).ToList();

                double min = 0;
                double max = 0;

                if (reducedList.Count > 0)
                {
                    min = reducedList.Select(x => double.Parse(x, CultureInfo.InvariantCulture)).Min();
                    max = reducedList.Select(x => double.Parse(x, CultureInfo.InvariantCulture)).Max();
                }

                if (max > 0)
                {
                    results[item.Key]
                        .ForEachIndexing((x, index) =>
                        x.Add("ApsSorting",
                        Math.Round((double.Parse(x["Game:APS"], CultureInfo.InvariantCulture) / max * 100), 0).ToString()));
                }
                else
                {
                    results[item.Key]
                        .ForEachIndexing((x, index) =>
                        x.Add("ApsSorting", "-1"));
                }
            }
        }

        private static void EnrichItemsWithLevelSorting(Dictionary<string, List<Dictionary<string, string>>> results, List<string> keys)
        {
            keys.ForEach(key => results[key] = results[key].OrderBy(x => GetItemLevel(x)).ToList());

            foreach (var item in results)
            {
                double classCount = results[item.Key].Count;
                results[item.Key].ForEachIndexing((x, index) => x.Add("LevelSorting", 
                Math.Round((index / classCount) * 100, 0).ToString()));
            }

            int GetItemLevel(Dictionary<string, string> item)
            {
                var iLvl = item["DropLevel"];
                int iLvlINT = string.IsNullOrEmpty(iLvl) ? 0 : Int32.Parse(iLvl);
                return iLvlINT;
            }
        }
    }
}