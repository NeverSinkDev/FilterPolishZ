using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilterCore.Constants;
using FilterEconomy.Model;
using FilterEconomy.Model.ItemAspects;
using FilterPolishUtil.Collections;
using FilterPolishUtil.Extensions;

namespace FilterEconomy.Request.Enrichment
{
    public class LowestPriceEnrichment : IDataEnrichment
    {
        public string DataKey => "LowestPrice";

        public void Enrich(string baseType, ItemList<NinjaItem> data)
        {
            List<NinjaItem> target = data;

            if (data.Count == 1)
            {
                data.LowestPrice = target[0].CVal;
                return;
            }

            var cleanedTarget = target.Where(x => !x.Aspects.Any(z => FilterConstants.GlobalIgnoreAspects.Contains(z.Name) || FilterConstants.IgnoredHighestPriceAspects.Contains(z.Name))).ToList();

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

            if (target.Count > 1)
            {
                if (target.All(x => x.Aspects.Any(z => !FilterConstants.GlobalIgnoreAspects.Contains(z.Name))))
                {
                    if (target.All(x => x.Aspects.Any(z => FilterConstants.IgnoredLowestPriceAspects.Contains(z.Name))))
                    {
                        data.LowestPrice = target.Min(x => x.CVal);
                        return;
                    }
                }

                var filteredData = data.Where(x => x.Aspects.All(z => !FilterConstants.IgnoredLowestPriceAspects.Contains(z.Name) && !FilterConstants.GlobalIgnoreAspects.Contains(z.Name))).ToList();
                if (filteredData.Count >= 1)
                {
                    target = filteredData;
                }
            }

            var price = target.Min(x => x.CVal);
            data.LowestPrice = price;
        }
    }
}
