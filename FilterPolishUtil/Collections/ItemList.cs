using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishUtil.Collections
{
    public class ItemList<T> : List<T>
    {
        public float HighestPrice { get; set; }

        public float RawAveragePrice { get; set; }

        public float LowestPrice { get; set; }
        
        public float ValueMultiplier { get; set; } = 1;

        public bool Valid { get; set; } = true;

        public AutoDictionary<int, float> ftCount { get; set; }
        public AutoDictionary<int, float> ftPrice { get; set; }

        public float GetPrice(int level)
        {
            if (level > 86)
            {
                return 0;
            }

            if (this.ftPrice?.ContainsKey(level) == true)
            {
                return this.ftPrice[level];
            }
            else
            {
                return GetPrice(level + 1);
            }
        }

        public float GetPriceMod(PricingMode pricingMode)
        {
            switch(pricingMode)
            {
                case PricingMode.lowest:
                    return this.LowestPrice;
                case PricingMode.highest:
                    return this.HighestPrice;
                case PricingMode.rawavg:
                    return this.RawAveragePrice;
                default:
                    return this.LowestPrice;
            }
        }
    }
}
