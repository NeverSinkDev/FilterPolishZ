using FilterCore.Constants;
using FilterEconomy.Facades;
using FilterEconomy.Model;
using FilterPolishUtil;
using FilterPolishUtil.Collections;
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
            List<KeyValuePair<float, string>> sortedFullPriceList = new List<KeyValuePair<float, string>>();

            var abstractionList = new InfluenceBTA();

            var itemLevel = 85;
            BaseBTA.ItemLevelContext = itemLevel;

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

                        currentSection[itemClass].BaseTypes.Add(item.Key, currentBaseType);
                        currentBaseType.enterPrice(itemLevel, item.Value, itemClass);
                    }
                }

                // Sort each list and omit the outliers
                var filteredValueList = new Dictionary<string, List<float>>();
                var filteredFullValueList = new Dictionary<string, List<float>>();

                foreach (var item in currentSection.Classes)
                {
                    item.Value.CreateList();
                    var confPrice = item.Value.BaseTypesList.Average(x => x.ConfValueList.Count == 0 ? 0 : x.ConfValueList[itemLevel]);
                    var fullPrice = item.Value.BaseTypesList.Average(x => x.FullValueList[itemLevel]);
                    sortedPriceList.Add(new KeyValuePair<float, string>(confPrice, $"[{itemLevel}]{section.Key} >> {item.Key} >> { confPrice } { fullPrice }"));
                }
            }

            var sortedValueList1 = sortedPriceList.OrderBy(x => x.Key).ToList();
            var sortedFullValueList1 = sortedFullPriceList.OrderBy(x => x.Key).ToList();

            foreach (var item in sortedValueList1)
            {
                LoggingFacade.LogDebug(item.Value);
            }

            foreach (var item in sortedFullValueList1)
            {
                LoggingFacade.LogDebug(item.Value);
            }
        }
    }

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

    public class ClassBTA
    {
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

        public Dictionary<string, BaseBTA> BaseTypes = new Dictionary<string, BaseBTA>();
        public List<BaseBTA> BaseTypesList { get; set; }
    }

    public class BaseBTA : IComparable
    {
        public static int ItemLevelContext;
        public Dictionary<int, float> ConfValueList = new Dictionary<int, float>();
        public Dictionary<int, float> FullValueList = new Dictionary<int, float>();
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

            if (ecoData.ValueMultiplier > 0.4f)
            {
                ConfValueList.Add(itemLevel, conf * price);
            }

            FullValueList.Add(itemLevel, conf * price);
        }
    }
}
