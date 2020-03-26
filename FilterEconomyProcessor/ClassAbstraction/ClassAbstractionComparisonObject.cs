using FilterPolishUtil;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilterEconomyProcessor.ClassAbstraction
{
    public static class ConcreteAbstractionComparisons
    {
        static ConcreteAbstractionComparisons()
        {
            ComparisonObjects.Add(new AbstractClassAbstractionComparisonObject()
            {
                MinItemLevel = 85,
                MaxItemLevel = 85,
                MinimumValidityRating = 1,
                MinimumFullPrice = FilterPolishConfig.InfluenceGroupT2BreakPoint,
                MinimumConfPrice = FilterPolishConfig.InfluenceGroupT2BreakPoint,
                Strategy = new BasicClassAbstractionComparisonStrategy(),
                RuleName = "ILVL 85, Full Validity, T2"
            });

            ComparisonObjects.Add(new AbstractClassAbstractionComparisonObject()
            {
                MinItemLevel = 82,
                MaxItemLevel = 82,
                MinimumValidityRating = 1,
                MinimumFullPrice = FilterPolishConfig.InfluenceGroupT2BreakPoint,
                MinimumConfPrice = FilterPolishConfig.InfluenceGroupT2BreakPoint,
                Strategy = new BasicClassAbstractionComparisonStrategy(),
                RuleName = "ILVL 82, Full Validity, T2"
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
            if (itemClass.ConfPrices[comparisonData.MaxItemLevel] < comparisonData.MinimumConfPrice)
            {
                return false;
            }

            if (itemClass.FullPrices[comparisonData.MaxItemLevel] < comparisonData.MinimumFullPrice)
            {
                return false;
            }

            if ((itemClass.ValidItems[comparisonData.MaxItemLevel]) / itemClass.BaseTypes.Count < comparisonData.MinimumValidityRating)
            {
                return false;
            }

            return true;
        }
    }
}
