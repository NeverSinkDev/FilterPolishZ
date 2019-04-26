﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilterCore.Constants;
using FilterEconomy.Model;
using FilterPolishUtil.Collections;
using FilterPolishUtil.Constants;
using FilterPolishUtil.Extensions;

namespace FilterEconomy.Request.Enrichment
{
    public class ShaperElderEnrichment : IDataEnrichment
    {
        public void Enrich(string baseType, ItemList<NinjaItem> data)
        {
            float confidence = 1;

            float averagePriceMinimum = 3;
            float approvedPricesMinimum = 8;
            float unhealthyPriceRange = 500;

            var totalQuant = data.Sum(x => x.IndexedCount);
            var minPrice = data.Min(x => x.CVal);
            var maxPrice = data.Max(x => x.CVal);
            var highestLevelPrice = data.OrderBy(x => x.LevelRequired).Last().CVal;
            var averagePrice = data.Sum(x => x.CVal * x.IndexedCount) / data.Sum(x => x.IndexedCount);

            var prices = data.OrderBy(x => x.LevelRequired).Select(x => x.CVal);
            var progression = prices.PairSelect((x, y) => y - x).Sum();

            // correct pricepeak
            confidence += AdjustConfidenceBasedOn(data, (s => highestLevelPrice < maxPrice), -0.2f, 0.1f);

            // min price relevant
            confidence += AdjustConfidenceBasedOn(data, (s => minPrice <= averagePriceMinimum), -0.15f, 0.1f);

            // count rules
            confidence += AdjustConfidenceBasedOn(data, (s => totalQuant <= 2), -0.5f, 0);
            confidence += AdjustConfidenceBasedOn(data, (s => totalQuant <= 4), -0.3f, 0);
            confidence += AdjustConfidenceBasedOn(data, (s => totalQuant <= 8), -0.1f, 0.05f);
            confidence += AdjustConfidenceBasedOn(data, (s => totalQuant <= 16), -0.05f, 0.05f);

            // progression rules
            confidence += AdjustConfidenceBasedOn(data, (s => progression <= averagePriceMinimum), 0f, 0.05f);
            confidence += AdjustConfidenceBasedOn(data, (s => progression <= 0),  -0.1f, 0.05f);
            confidence += AdjustConfidenceBasedOn(data, (s => progression <= -5),  -0.1f, 0);
            confidence += AdjustConfidenceBasedOn(data, (s => progression <= -20), -0.1f, 0);

            // outlier rules
            confidence += AdjustConfidenceBasedOn(data, (s => minPrice < averagePriceMinimum && maxPrice > FilterPolishConstants.T1BaseTypeBreakPoint), -0.1f, 0);
            confidence += AdjustConfidenceBasedOn(data, (s => maxPrice >= unhealthyPriceRange), -0.1f, 0);
            confidence += AdjustConfidenceBasedOn(data, (s => maxPrice / minPrice > 100),-0.1f, 0);
            confidence += AdjustConfidenceBasedOn(data, (s => maxPrice / minPrice > 75), -0.1f, 0);
            confidence += AdjustConfidenceBasedOn(data, (s => maxPrice / minPrice > 50), -0.1f, 0);
            confidence += AdjustConfidenceBasedOn(data, (s => maxPrice / minPrice > 25), -0.1f, 0);

            // item info based rules

            Dictionary<string, string> itemInfo = null;
            if (BaseTypeDataProvider.BaseTypeData.ContainsKey(baseType))
            {
                itemInfo = BaseTypeDataProvider.BaseTypeData[baseType];
                int dropLevel = int.Parse(itemInfo["DropLevel"]);
                string itemClass = itemInfo["Class"].ToLower();

                if (!FilterConstants.DropLevelIgnoredClasses.Contains(itemClass) && dropLevel != 0)
                {
                    confidence += AdjustConfidenceBasedOn(data, (s => dropLevel < 70),  0, 0.05f);
                    confidence += AdjustConfidenceBasedOn(data, (s => dropLevel < 60), -0.05f, 0.05f);
                    confidence += AdjustConfidenceBasedOn(data, (s => dropLevel < 50), -0.05f, 0);
                    confidence += AdjustConfidenceBasedOn(data, (s => dropLevel < 40), -0.10f, 0);
                    confidence += AdjustConfidenceBasedOn(data, (s => dropLevel < 30), -0.10f, 0);
                    confidence += AdjustConfidenceBasedOn(data, (s => dropLevel < 20), -0.15f, 0);
                    confidence += AdjustConfidenceBasedOn(data, (s => dropLevel < 10), -0.2f, 0);
                }
            }
            else
            {
                Debug.WriteLine($"Missing BaseType: {baseType}");
            }

            if (confidence <= 0.35f)
            {
                data.Valid = false;
            }

            data.ValueMultiplier = confidence;
            data.ftPrice = new AutoDictionary<int, float>();
            data.ForEach(x => data.ftPrice[(int)x.LevelRequired] = x.CVal);
        }

        private float AdjustConfidenceBasedOn(ItemList<NinjaItem> target, Func<ItemList<NinjaItem>, bool> rule, float factor, float antifactor)
        {
            if (rule(target))
            {
                return factor;
            }
            else
            {
                return antifactor;
            }
        }
    }
}
