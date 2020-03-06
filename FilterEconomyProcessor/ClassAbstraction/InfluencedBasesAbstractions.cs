using FilterCore.Constants;
using FilterEconomy.Facades;
using FilterEconomy.Model;
using FilterPolishUtil;
using FilterPolishUtil.Collections;
using FilterPolishUtil.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FilterEconomyProcessor.ClassAbstraction
{
    public class InfluencedBasesAbstractions
    {
        public static InfluenceBTA InfluencedItemInformation { get; set; }

        public void Execute()
        {
            InfluencedItemInformation = null;
            LoggingFacade.LogInfo($"Performing Influenced Class Abstractions");

            List<KeyValuePair<float, string>> sortedConfList = new List<KeyValuePair<float, string>>();
            List<KeyValuePair<float, string>> sortedPriceList = new List<KeyValuePair<float, string>>();
            List<KeyValuePair<float, string>> sortedFullPriceList = new List<KeyValuePair<float, string>>();

            var abstractionList = new InfluenceBTA();

            foreach (var section in EconomyRequestFacade.GetInstance().EconomyTierlistOverview)
            {
                if (!section.Key.Contains("rare"))
                {
                    continue;
                }

                var currentSection = abstractionList.AddNewSection(section.Key);

                foreach (var item in section.Value)
                {
                    if (BaseTypeDataProvider.BaseTypeData.ContainsKey(item.Key))
                    {
                        var currentBaseType = new BaseBTA();
                        currentBaseType.Name = item.Key;

                        for (int ilvl = 80; ilvl <= 86; ilvl++)
                        {
                            BaseBTA.ItemLevelContext = ilvl;
                            // Atlas bases falsify the results, so we're skipping them.
                            if (FilterPolishConfig.SpecialBases.Contains(item.Key))
                            {
                                currentBaseType.SpecialBase = true;
                            }

                            // We need to access the facade to augment eco information with basetypedata
                            var itemInfo = BaseTypeDataProvider.BaseTypeData[item.Key];

                            var dropLevel = int.Parse(itemInfo["DropLevel"]);
                            var itemClass = itemInfo["Class"].ToLower();

                            // New section treatment
                            if (!currentSection.Classes.ContainsKey(itemClass))
                            {
                                currentSection.AddNewClass(itemClass);
                            }

                            currentSection[itemClass][item.Key] = currentBaseType;
                            currentBaseType.enterPrice(ilvl, item.Value, itemClass);
                        }
                    }
                }


                // Sort each list and omit the outliers
                var filteredValueList = new Dictionary<string, List<float>>();
                var filteredFullValueList = new Dictionary<string, List<float>>();

                for (int ilvl = 80; ilvl <= 86; ilvl++)
                {
                    BaseBTA.ItemLevelContext = ilvl;
                    foreach (var item in currentSection.Classes)
                    {
                        item.Value.CreateList();
                        var confPrice = item.Value.BaseTypesList
                            .Where(x => !x.SpecialBase).Average(x => x.ConfValueList.Count == 0 ? 0 : x.ConfValueList[ilvl]);

                        var fullPrice = item.Value.BaseTypesList
                            .Where(x => !x.SpecialBase).Average(x => x.FullValueList[ilvl]);

                        var validPrices = item.Value.BaseTypesList.Count(x => x.ConfValueList[ilvl] > 0.5f);

                        item.Value.AvgConfidence = item.Value.BaseTypes.Average(x => x.Value.Confidence);
                        item.Value.ConfPrices.Add(ilvl, confPrice);
                        item.Value.FullPrices.Add(ilvl, fullPrice);
                        item.Value.ValidItems.Add(ilvl, validPrices);

                        if (ilvl == 86)
                        {
                            sortedPriceList.Add(new KeyValuePair<float, string>(confPrice, $"[{ilvl}]{section.Key} >> {item.Key} >> { confPrice } { fullPrice }"));
                        }
                    }
                }


            }

            var sortedValueList1 = sortedPriceList.OrderBy(x => x.Key).ToList();

            foreach (var item in sortedValueList1)
            {
                LoggingFacade.LogDebug(item.Value);
            }

            InfluencedItemInformation = abstractionList;
        }
    }

    /// <summary>
    /// Represents information about all influences in the current economy
    /// </summary>
    public class InfluenceBTA
    {
        public ClassListBTA this[string influenceName]
        {
            get
            {
                return InfluenceTypes[influenceName];
            }
            set
            {
                InfluenceTypes[influenceName] = value;
            }
        }


        public ClassListBTA AddNewSection(string name)
        {
            var bta = new ClassListBTA();
            this.InfluenceTypes.Add(name, bta);
            return bta;
        }

        Dictionary<string, ClassListBTA> InfluenceTypes = new Dictionary<string, ClassListBTA>();
    }

    /// <summary>
    /// Represents information about a whole influence in the economy.
    /// </summary>
    public class ClassListBTA
    {
        public ClassBTA this[string className]
        {
            get
            {
                return Classes[className];
            }
            set
            {
                if (!Classes.ContainsKey(className))
                {
                    AddNewClass(className);
                }

                Classes[className] = value;
            }
        }

        public Dictionary<string, ClassBTA> Classes = new Dictionary<string, ClassBTA>();

        internal ClassBTA AddNewClass(string itemClass)
        {
            var bta = new ClassBTA();
            this.Classes.Add(itemClass, bta);
            return bta;
        }
    }

    /// <summary>
    /// Represents abstracted information about one class in the influenced tier economy
    /// </summary>
    [DebuggerDisplay("{Validity,nq}")]
    public class ClassBTA
    {
        public Dictionary<string, BaseBTA> BaseTypes = new Dictionary<string, BaseBTA>();
        public List<BaseBTA> BaseTypesList { get; set; }
        public Dictionary<int, float> ConfPrices = new Dictionary<int, float>();
        public Dictionary<int, float> FullPrices = new Dictionary<int, float>();
        public Dictionary<int, int> ValidItems = new Dictionary<int, int>();

        public string Validity
        {
            get
            {
                return $"{ValidItems[80]},{ValidItems[81]},{ValidItems[82]},{ValidItems[83]},{ValidItems[84]},{ValidItems[85]},{ValidItems[86]}";
            }
        }

        public float AvgConfidence;

        public BaseBTA this[string baseTypeName]
        {
            get
            {
                return BaseTypes[baseTypeName];
            }
            set
            {
                if (!BaseTypes.ContainsKey(baseTypeName))
                {
                    AddNewBaseType(baseTypeName);
                }

                BaseTypes[baseTypeName] = value;
            }
        }

        private void AddNewBaseType(string baseTypeName)
        {
            this.BaseTypes.Add(baseTypeName, new BaseBTA());
        }

        public void CreateList()
        {
            this.BaseTypesList = BaseTypes.Values.OrderBy(x => x).ToList();
        }
    }

    /// <summary>
    /// Represents information about a single basetype in the influenced tier economy
    /// </summary>
    public class BaseBTA : IComparable
    {
        public static int ItemLevelContext;
        public Dictionary<int, float> ConfValueList = new Dictionary<int, float>();
        public Dictionary<int, float> FullValueList = new Dictionary<int, float>();
        public float Confidence = 0;
        public bool EnrichmentValidity = true;
        public ItemList<NinjaItem> EcoData { get; private set; }
        public bool SpecialBase = false;
        public string Name { get; internal set; }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            var secondbase = obj as BaseBTA;

            if (secondbase.ConfValueList.Count == 0)
            {
                return 1;
            }

            if (this.ConfValueList.Count == 0)
            {
                return 0;
            }

            return this.ConfValueList[ItemLevelContext].CompareTo(secondbase.ConfValueList[ItemLevelContext]);
        }

        internal void enterPrice(int itemLevel, ItemList<NinjaItem> ecoData, string itemClass)
        {
            var conf = ecoData.ValueMultiplier;
            var price = ecoData.GetPrice(itemLevel);

            this.Confidence = ecoData.ValueMultiplier;
            this.EnrichmentValidity = ecoData.Valid;
            this.EcoData = ecoData;

            if (ecoData.ValueMultiplier > 0.5f)
            {
                this.ConfValueList.Add(itemLevel, conf * price);
            }

            this.FullValueList.Add(itemLevel, conf * price);
        }
    }
}
