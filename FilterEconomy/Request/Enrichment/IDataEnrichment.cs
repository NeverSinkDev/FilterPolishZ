using FilterEconomy.Model;
using FilterPolishUtil.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterEconomy.Request.Enrichment
{
    public interface IDataEnrichment
    {
        string DataKey { get; }
        void Enrich(string baseType, ItemList<NinjaItem> data);
    }
}
