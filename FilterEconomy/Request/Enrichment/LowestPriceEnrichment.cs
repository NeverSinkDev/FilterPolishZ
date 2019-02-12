using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilterEconomy.Model;
using FilterPolishUtil.Collections;

namespace FilterEconomy.Request.Enrichment
{
    public class LowestPriceEnrichment : IDataEnrichment
    {
        public string DataKey => "LowestPrice";

        public void Enrich(string baseType, ItemList<NinjaItem> data)
        {
            var price = data.Min(x => x.CVal);
            data.LowestPrice = price;
        }
    }
}
