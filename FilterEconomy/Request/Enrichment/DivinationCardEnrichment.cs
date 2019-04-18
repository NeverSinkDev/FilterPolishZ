using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilterEconomy.Model;
using FilterPolishUtil.Collections;

namespace FilterEconomy.Request.Enrichment
{
    public class DivinationCardEnrichment : IDataEnrichment
    {
        public string DataKey => "DivinationPrice";

        public void Enrich(string baseType, ItemList<NinjaItem> data)
        {
            if (data.Count != 1)
            {
                throw new Exception($"{baseType} Divination Card List has non-Single count!");
                return;
            }
            var target = data[0];

            if (data.Count == 1)
            {
                data.LowestPrice = target.CVal;

                float multiplier = 1;
                multiplier += this.AdjustPriceBasedOn(target, new Func<NinjaItem, bool>((NinjaItem s) => s.StackSize <= 1), 0.2f);
                multiplier += this.AdjustPriceBasedOn(target, new Func<NinjaItem, bool>((NinjaItem s) => s.StackSize <= 2), 0.1f);
                multiplier += this.AdjustPriceBasedOn(target, new Func<NinjaItem, bool>((NinjaItem s) => s.StackSize <= 3), 0.1f);
                multiplier += this.AdjustPriceBasedOn(target, new Func<NinjaItem, bool>((NinjaItem s) => s.StackSize >= 6), -0.05f);
                multiplier += this.AdjustPriceBasedOn(target, new Func<NinjaItem, bool>((NinjaItem s) => s.StackSize >= 8), -0.1f);
                multiplier += this.AdjustPriceBasedOn(target, new Func<NinjaItem, bool>((NinjaItem s) => s.StackSize >= 10), -0.1f);
                multiplier += this.AdjustPriceBasedOn(target, new Func<NinjaItem, bool>((NinjaItem s) => s.StackSize >= 12), -0.1f);
                multiplier += this.AdjustPriceBasedOn(target, new Func<NinjaItem, bool>((NinjaItem s) => s.HasAspect("LargeRandomPoolAspect")), 0.1f);
                multiplier += this.AdjustPriceBasedOn(target, new Func<NinjaItem, bool>((NinjaItem s) => s.HasAspect("CurrencyTypeAspect")), 0.1f);
                data.ValueMultiplier = multiplier;
            }

        }

        public float AdjustPriceBasedOn(NinjaItem item, Func<NinjaItem,bool> rule, float factor)
        {
            if (rule(item))
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
