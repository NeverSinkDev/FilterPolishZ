using FilterPolishUtil.Extensions;
using FilterPolishUtil.Model;
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

        public static Dictionary<string, Dictionary<string, string>> BaseTypeData { get; set; } = new Dictionary<string, Dictionary<string, string>>();
        public static Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>> BaseTypeTieringMatrixData { get; private set; } = new Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>>();

        public static Dictionary<string, List<Dictionary<string, string>>> ClassBasedTierList = new Dictionary<string, List<Dictionary<string, string>>>();

        public static void Initialize()
        {
            BaseTypeData = LoadData();
            ClassBasedTierList = CreateClassBasedList();
        }

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

            CreateLevelSortedBaseTypeList(results);

            EnrichItemsWithBestBaseTypeInformation(results);

            return results;

        }

        private static void CreateLevelSortedBaseTypeList(Dictionary<string, List<Dictionary<string, string>>> results)
        {
            // INPUT: SORTED LIST<PROP,VALUE>
            // ARMORTYPE(weapon or EV/ES) => { CLASS : LIST<PROP, VALUE> (sorted by droplevel) }
            var finalgroups = new Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>>();

            foreach (var itemtype in results)
            {
                var subgroups = new Dictionary<string, List<Dictionary<string, string>>>();

                if (!FilterPolishUtil.FilterPolishConfig.GearClasses.Contains(itemtype.Key))
                {
                    continue;
                }

                if (FilterPolishUtil.FilterPolishConfig.BestBaseCheckIgnore.Contains(itemtype.Key.ToLower()))
                {
                    // rings, amulets and belts don't need any indication about the best base
                    continue;
                }

                if (!itemtype.Value.FirstOrDefault().ContainsKey("LevelSorting"))
                {
                    continue;
                }

                var output = itemtype.Value.Where(x => !FilterPolishUtil.FilterPolishConfig.SpecialBases.Contains(x["BaseType"]) ).OrderByDescending(y => float.Parse(y["LevelSorting"])).ToList();

                if (itemtype.Key == "Boots" || itemtype.Key == "Body Armours" || itemtype.Key == "Helmets" || itemtype.Key == "Shields" || itemtype.Key == "Gloves")
                {
                    subgroups = output.Subdivide(new List<Tuple<string, Func<Dictionary<string, string>, bool>>>()
                    {
                        new Tuple<string, Func<Dictionary<string, string>, bool>>("ar", x => IsArmor(x)),
                        new Tuple<string, Func<Dictionary<string, string>, bool>>("ev", x => IsEvasion(x)),
                        new Tuple<string, Func<Dictionary<string, string>, bool>>("es", x => IsEnergyShield(x)),
                        new Tuple<string, Func<Dictionary<string, string>, bool>>("arev", x => IsArmorEvasion(x)),
                        new Tuple<string, Func<Dictionary<string, string>, bool>>("ares", x => IsArmorEnergyShield(x)),
                        new Tuple<string, Func<Dictionary<string, string>, bool>>("eves", x => IsEvasionEnergyShield(x))
                    });
                }
                else
                {
                    subgroups.Add("all", output);
                }

                foreach (var item in subgroups)
                {

                    string key1 = item.Key;
                    string key2 = itemtype.Key;
                    if (item.Key == "all")
                    {
                        key1 = itemtype.Key;
                        key2 = item.Key;
                    }

                    // add the armor type
                    if (!finalgroups.ContainsKey(key1))
                    {
                        finalgroups.Add(key1, new Dictionary<string, List<Dictionary<string, string>>>());
                    }

                    if (!finalgroups[key1].ContainsKey(key2))
                    {
                        finalgroups[key1].Add(key2, new List<Dictionary<string, string>>());
                    }

                    finalgroups[key1][key2] = item.Value;
                }
            }

            BaseTypeTieringMatrixData = finalgroups;
        }

        private static bool IsEvasionEnergyShield(Dictionary<string, string> x)
        {
            return x["Game:Armour"] == "" && x["Game:Evasion"] != "" && x["Game:Energy Shield"] != "";
        }

        private static bool IsArmorEnergyShield(Dictionary<string, string> x)
        {
            return x["Game:Armour"] != "" && x["Game:Evasion"] == "" && x["Game:Energy Shield"] != "";
        }

        private static bool IsArmorEvasion(Dictionary<string, string> x)
        {
            return x["Game:Armour"] != "" && x["Game:Evasion"] != "" && x["Game:Energy Shield"] == "";
        }

        private static bool IsEnergyShield(Dictionary<string, string> x)
        {
            return x["Game:Armour"] == "" && x["Game:Evasion"] == "" && x["Game:Energy Shield"] != "";
        }

        private static bool IsEvasion(Dictionary<string, string> x)
        {
            return x["Game:Armour"] == "" && x["Game:Evasion"] != "" && x["Game:Energy Shield"] == "";
        }

        private static bool IsArmor(Dictionary<string, string> x)
        {
            return x["Game:Armour"] != "" && x["Game:Evasion"] == "" && x["Game:Energy Shield"] == "";
        }

        private static void EnrichItemsWithBestBaseTypeInformation(Dictionary<string, List<Dictionary<string, string>>> results)
        {
            HashSet<string> resultingBases = new HashSet<string>();

            foreach (var itemtype in results)
            {
                if (!FilterPolishUtil.FilterPolishConfig.GearClasses.Contains(itemtype.Key))
                {
                    continue;
                }

                if (FilterPolishUtil.FilterPolishConfig.BestBaseCheckIgnore.Contains(itemtype.Key.ToLower()))
                {
                    // rings, amulets and belts don't need any indication about the best base
                    continue;
                }

                if (!itemtype.Value.FirstOrDefault().ContainsKey("LevelSorting"))
                {
                    continue;
                }

                var itemgroups = new Dictionary<string, List<Dictionary<string, string>>>();

                if (itemtype.Key == "Boots" || itemtype.Key == "Body Armours" || itemtype.Key == "Helmets" || itemtype.Key == "Shields" || itemtype.Key == "Gloves")
                {
                    
                    itemgroups = itemtype.Value.Subdivide(new List<Tuple<string, Func<Dictionary<string, string>, bool>>>()
                    {
                        new Tuple<string, Func<Dictionary<string, string>, bool>>("ar", x => IsArmor(x)),
                        new Tuple<string, Func<Dictionary<string, string>, bool>>("ev", x => IsEvasion(x)),
                        new Tuple<string, Func<Dictionary<string, string>, bool>>("es", x => IsEnergyShield(x)),
                        new Tuple<string, Func<Dictionary<string, string>, bool>>("arev", x => IsArmorEvasion(x)),
                        new Tuple<string, Func<Dictionary<string, string>, bool>>("ares", x => IsArmorEnergyShield(x)),
                        new Tuple<string, Func<Dictionary<string, string>, bool>>("eves", x => IsEvasionEnergyShield(x))
                    });
                }
                else
                {
                    itemgroups.Add("all", itemtype.Value);
                }

                foreach (var itemgroup in itemgroups)
                {
                    var rawingridients = itemgroup.Value
                        .Where(x => !FilterPolishUtil.FilterPolishConfig.SpecialBases.Contains(x["BaseType"]))
                        .Select(x => new { value = x, lvlSort = float.Parse(x["LevelSorting"]), apsSort = float.Parse(x["ApsSorting"]) });

                    var rawresults = rawingridients
                        .MaxBy(x => x.lvlSort).ToList();

                    foreach (var item in rawresults)
                    {
                        resultingBases.AddIfNew(item.value["BaseType"]);
                    }

                    // get the best high speed weapons
                    if (itemtype.Key != "Rune Daggers" && itemtype.Key != "Staves")
                    {
                        var rawApsResults = rawingridients.Where(x => x.apsSort > 0.5f)
                            .MaxBy(x => (x.apsSort * x.apsSort * x.apsSort) * x.lvlSort).ToList();

                        foreach (var item in rawApsResults)
                        {
                            resultingBases.AddIfNew(item.value["BaseType"]);
                        }
                    }
                }
            }

            foreach (var item in FilterPolishUtil.FilterPolishConfig.SpecialBases)
            {
                resultingBases.AddIfNew(item);
            }

            foreach (var item in FilterPolishUtil.FilterPolishConfig.ExtraBases)
            {
                resultingBases.AddIfNew(item);
            }

            FilterPolishUtil.FilterPolishConfig.TopBases = resultingBases;
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
                    foreach (var basetype in results[item.Key])
                    {
                        if (string.IsNullOrEmpty(basetype["Game:APS"]))
                        {
                            LoggingFacade.LogWarning($"APS information missing for basetype {basetype["BaseType"]}",true);
                            basetype["Game:APS"] = min.ToString();
                        }
                    }

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

        public static Dictionary<string,string> Get(string name)
        {
            return BaseTypeDataProvider.BaseTypeData[name];
        }

        public static bool Has(string name)
        {
            return BaseTypeDataProvider.BaseTypeData.ContainsKey(name);
        }
    }
}