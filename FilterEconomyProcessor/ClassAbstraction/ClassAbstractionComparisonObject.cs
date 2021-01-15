using FilterPolishUtil;
using FilterPolishUtil.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FilterEconomyProcessor.ClassAbstraction
{
    public static class ConcreteAbstractionComparisons
    {
        static ConcreteAbstractionComparisons()
        {
            ComparisonObjects.Add(new AbstractClassAbstractionComparisonObject()
            {
                MinItemLevel = 86,
                MaxItemLevel = 86,
                MinimumValidityRating = 0.55f,
                MinimumFullPrice = FilterPolishConfig.InfluenceGroupT2BreakPoint * 1f,
                MinimumConfPrice = FilterPolishConfig.InfluenceGroupT2BreakPoint * 2.5f,
                Strategy = new BasicClassAbstractionComparisonStrategy(),
                RuleName = "ILVL 86, High Threshhold",
                PriceTreatment = Treatment.highest,
                ValidityTreatment = Treatment.lowest
            });

            ComparisonObjects.Add(new AbstractClassAbstractionComparisonObject()
            {
                MinItemLevel = 85,
                MaxItemLevel = 86,
                MinimumValidityRating = 0.75f,
                MinimumFullPrice = FilterPolishConfig.InfluenceGroupT1BreakPoint * 0.7f,
                MinimumConfPrice = FilterPolishConfig.InfluenceGroupT1BreakPoint * 0.8f,
                Strategy = new BasicClassAbstractionComparisonStrategy(),
                RuleName = "ILVL 85-86, High Confidence, T1",
                PriceTreatment = Treatment.lowest,
                ValidityTreatment = Treatment.highest
            });

            ComparisonObjects.Add(new AbstractClassAbstractionComparisonObject()
            {
                MinItemLevel = 82,
                MaxItemLevel = 83,
                MinimumValidityRating = 0.75f,
                MinimumFullPrice = FilterPolishConfig.InfluenceGroupT1BreakPoint * 0.7f,
                MinimumConfPrice = FilterPolishConfig.InfluenceGroupT1BreakPoint * 0.8f,
                Strategy = new BasicClassAbstractionComparisonStrategy(),
                RuleName = "ILVL 82-83, High Confidence, T1",
                PriceTreatment = Treatment.lowest,
                ValidityTreatment = Treatment.highest
            });

            ComparisonObjects.Add(new AbstractClassAbstractionComparisonObject()
            {
                MinItemLevel = 85,
                MaxItemLevel = 86,
                MinimumValidityRating = 0.66f,
                MinimumFullPrice = FilterPolishConfig.InfluenceGroupT2BreakPoint * 0.66f,
                MinimumConfPrice = FilterPolishConfig.InfluenceGroupT2BreakPoint,
                Strategy = new BasicClassAbstractionComparisonStrategy(),
                RuleName = "ILVL 85-86, High Confidence, T2",
                PriceTreatment = Treatment.lowest,
                ValidityTreatment = Treatment.highest
            });

            ComparisonObjects.Add(new AbstractClassAbstractionComparisonObject()
            {
                MinItemLevel = 82,
                MaxItemLevel = 83,
                MinimumValidityRating = 0.66f,
                MinimumFullPrice = FilterPolishConfig.InfluenceGroupT2BreakPoint * 0.66f,
                MinimumConfPrice = FilterPolishConfig.InfluenceGroupT2BreakPoint,
                Strategy = new BasicClassAbstractionComparisonStrategy(),
                RuleName = "ILVL 82-83, High Confidence, T2",
                PriceTreatment = Treatment.lowest,
                ValidityTreatment = Treatment.highest
            });
        }

        /// <summary>
        /// To test if a class of influenced items is expensive, we're using a list of "ComparisonObjects" that compare themself to a class of items
        /// The CompairsonObjects are created on static startup and contain a strategy that includes the comparsonlogic
        /// </summary>
        public static List<AbstractClassAbstractionComparisonObject> ComparisonObjects { get; set; } = new List<AbstractClassAbstractionComparisonObject>();

    }

    public class AbstractClassAbstractionComparisonObject
    {
        public int MinItemLevel { get; set; }
        public int MaxItemLevel { get; set; }
        public float MinimumValidityRating { get; set; }
        public float MinimumConfPrice { get; set; }
        public float MinimumFullPrice { get; set; }
        public string ResultingTier { get; set; }
        public string RuleName { get; set; }
        public Treatment PriceTreatment { get; set; }
        public Treatment ValidityTreatment { get; set; }
        public IClassAbstractionComparisonStrategy Strategy { get; set; }

        /// <summary>
        /// Test if a itemclass is fitting for the strategy.
        /// </summary>
        /// <param name="itemClass"></param>
        /// <returns></returns>
        public bool Execute(ClassBTA itemClass)
        {
            return this.Strategy.TestItemClass(itemClass, this);
        }
    }

    public interface IClassAbstractionComparisonStrategy
    {
        bool TestItemClass(ClassBTA itemClass, AbstractClassAbstractionComparisonObject comparisonData);
    }

    /// <summary>
    /// The basic comparison - requires perfect validity, confprice and fullprice.
    /// </summary>
    public class BasicClassAbstractionComparisonStrategy : IClassAbstractionComparisonStrategy
    {
        public bool TestItemClass(ClassBTA itemClass, AbstractClassAbstractionComparisonObject comparisonData)
        {
            var confPrice = LevelAction(x => itemClass.ConfPrices[x], comparisonData, comparisonData.PriceTreatment);
            var fullPrice = LevelAction(x => itemClass.FullPrices[x], comparisonData, comparisonData.PriceTreatment);
            var validCount = LevelAction(x => itemClass.ValidItems[x], comparisonData, comparisonData.ValidityTreatment);

            if (confPrice.Item2 == false || fullPrice.Item2 == false || validCount.Item2 == false)
            {
                LoggingFacade.LogDebug($"No results found for:{ itemClass.ClassName }");
                return false;
            }

            if (confPrice.Item1 < comparisonData.MinimumConfPrice)
            {
                return false;
            }

            if (fullPrice.Item1 < comparisonData.MinimumFullPrice)
            {
                return false;
            }

            if ((float)(validCount.Item1) / (float)itemClass.BaseTypes.Count < comparisonData.MinimumValidityRating)
            {
                return false;
            }

            return true;
        }

        public Tuple<T, bool?> LevelAction<T>(Func<int,T> action, AbstractClassAbstractionComparisonObject strategy, Treatment treatment)
        {
            if (strategy.MaxItemLevel == strategy.MinItemLevel)
            {
                return new Tuple<T,bool?>(action(strategy.MaxItemLevel), null);
            }

            List<T> results = new List<T>();
            for (int lvl = strategy.MinItemLevel; lvl <= strategy.MaxItemLevel; lvl++)
            {
                results.Add(action(lvl));
            }

            if (results.Count == 0)
            {
                return new Tuple<T, bool?>(default(T), false);
            }

            if (treatment == Treatment.highest)
            {
                return new Tuple<T, bool?>(results.Max(x => x), true);
            }

            if (treatment == Treatment.lowest)
            {
                return new Tuple<T, bool?>(results.Min(x => x), true);
            }

            return new Tuple<T, bool?>(results.First(x => x != null), true);
        }
    }

    public enum Treatment
    {
        highest,
        lowest
    }
}
