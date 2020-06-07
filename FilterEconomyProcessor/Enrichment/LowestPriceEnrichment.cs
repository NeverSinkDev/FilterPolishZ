using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilterCore;
using FilterCore.Constants;
using FilterEconomy.Model;
using FilterEconomy.Model.ItemAspects;
using FilterPolishUtil;
using FilterPolishUtil.Collections;
using FilterPolishUtil.Extensions;

namespace FilterEconomyProcessor.Enrichment
{
    public class LowestPriceEnrichment : IDataEnrichment
    {
        public void Enrich(string baseType, ItemList<NinjaItem> data)
        {
            List<NinjaItem> target = data;
            
            // calculate raw average values, before performing the lowest value calculation (mostly for unique maps)
            var validItems = target.Where(x => x != null && x.CVal > 0).ToList();
            if (validItems.Count > 0)
            {
                data.RawAveragePrice = validItems.Average(x => x.CVal);
            }
            else
            {
                data.RawAveragePrice = 0;
            }

            if (data.Count == 1)
            {
                data.LowestPrice = target[0].CVal;
                return;
            }

            var cleanedTarget = target.Where(x => !x.Aspects.Any(z => FilterPolishConfig.GlobalIgnoreAspects.Contains(z.Name) || FilterPolishConfig.IgnoredHighestPriceAspects.Contains(z.Name))).ToList();

            target = cleanedTarget;
            if (target.Count == 1)
            {
                data.LowestPrice = target[0].CVal;
                return;
            }
            
            if (target.Count == 0)
            {
                target = data;
            }

            var price = target.Min(x => x.CVal);
            data.LowestPrice = price;
        }
    }
}
