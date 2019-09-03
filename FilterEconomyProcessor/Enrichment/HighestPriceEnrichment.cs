using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilterCore;
using FilterCore.Constants;
using FilterEconomy.Model;
using FilterPolishUtil.Collections;

namespace FilterEconomyProcessor.Enrichment
{
    public class HighestPriceEnrichment : IDataEnrichment
    {
        public void Enrich(string baseType, ItemList<NinjaItem> data)
        {
            List<NinjaItem> target = data;

            if (data.Count > 1)
            {
                if (target.All(x => x.Aspects.All(z => !FilterGenerationConfig.GlobalIgnoreAspects.Contains(z.Name))))
                {
                    if (target.All(x => x.Aspects.Any(z => FilterGenerationConfig.IgnoredHighestPriceAspects.Contains(z.Name))))
                    {
                        data.HighestPrice = target.Max(x => x.CVal);
                        return;
                    }
                }

                var filteredData = data.Where(x => x.Aspects.All(z => !FilterGenerationConfig.IgnoredHighestPriceAspects.Contains(z.Name) && !FilterGenerationConfig.GlobalIgnoreAspects.Contains(z.Name))).ToList();
                if (filteredData.Count >= 1)
                {
                    target = filteredData;
                }
            }

            var price = target.Max(x => x.CVal);
            data.HighestPrice = price;
        }
    }
}
