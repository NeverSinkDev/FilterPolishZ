using FilterEconomy.Model;
using FilterPolishUtil.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterEconomy.Model
{
    public interface IDataEnrichment
    {
        void Enrich(string baseType, ItemList<NinjaItem> data);
    }
}
