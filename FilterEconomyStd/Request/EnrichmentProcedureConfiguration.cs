using FilterEconomy.Request.Enrichment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterEconomy.Request
{
    public static class EnrichmentProcedureConfiguration
    {
        public static Dictionary<string, List<IDataEnrichment>> EnrichmentProcedures = new Dictionary<string, List<IDataEnrichment>>();
    }
}
