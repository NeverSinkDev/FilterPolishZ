using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilterEconomy.Model;
using FilterPolishUtil.Collections;
using FilterPolishUtil.Extensions;

namespace FilterEconomy.Request.Enrichment
{
    public class ShaperElderEnrichment : IDataEnrichment
    {
        public void Enrich(string baseType, ItemList<NinjaItem> data)
        {
            float confidence = 1;

            float averagePriceMinimum = 2;
            float approvedPricesMinimum = 8;
            float minCount = 4;
            float minExoticCheckCount = 16;
            float unhealthyCount = 250;

            var totalQuant = data.Sum(x => x.IndexedCount);
            var minPrice = data.Min(x => x.CVal);
            var maxPrice = data.Max(x => x.CVal);
            var averagePrice = data.Sum(x => x.CVal * x.IndexedCount) / data.Sum(x => x.IndexedCount);

            var prices = data.OrderBy(x => x.LevelRequired).Select(x => x.CVal);
            var progression = prices.PairSelect((x, y) => y - x).Sum();

            confidence += AdjustConfidenceBasedOn(data, (s => minPrice <= averagePriceMinimum), -0.5f);

            // count rules
            confidence += AdjustConfidenceBasedOn(data, (s => totalQuant <= minCount), -0.5f);
            confidence += AdjustConfidenceBasedOn(data, (s => totalQuant <= (minCount * 5) && totalQuant <= unhealthyCount), 0.1f);
            confidence += AdjustConfidenceBasedOn(data, (s => totalQuant >= unhealthyCount), -0.1f);

            // progression rules
            confidence += AdjustConfidenceBasedOn(data, (s => progression <= 0), -0.3f);
            confidence += AdjustConfidenceBasedOn(data, (s => progression >= averagePriceMinimum), 0.1f);
            confidence += AdjustConfidenceBasedOn(data, (s => progression >= approvedPricesMinimum), 0.1f);

            confidence += AdjustConfidenceBasedOn(data, new Func<ItemList<NinjaItem>, bool>((ItemList<NinjaItem> s) => s.Count > 0), 1);

            data.ftPrice = new AutoDictionary<int, float>();
            data.ForEach(x => data.ftPrice[(int)x.LevelRequired] = x.CVal);

            Debug.WriteLine($"{baseType} --> {string.Join(" ", prices.Select(x => x.ToString()))}--->{progression}");
        }

        private float AdjustConfidenceBasedOn(ItemList<NinjaItem> target, Func<ItemList<NinjaItem>, bool> rule, float factor)
        {
            if (rule(target))
            {
                return factor;
            }
            else
            {
                return 0;
            }
        }
    }
}
