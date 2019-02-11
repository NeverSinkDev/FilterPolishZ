using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilterEconomy.Model;
using FilterPolishUtil.Collections;

namespace FilterEconomy.Request.Enrichment
{
    public class HighestPriceEnrichment : IDataEnrichment
    {
        public string DataKey => "HighestPrice";

        public void Enrich(string baseType, ItemList<NinjaItem> data)
        {
            var price = data.Max(x => x.CVal);
            data.MetaData[DataKey] = price.ToString();
        }
    }
}
