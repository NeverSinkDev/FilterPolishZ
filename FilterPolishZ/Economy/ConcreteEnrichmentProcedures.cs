using FilterEconomy.Request;
using FilterEconomy.Request.Enrichment;
using FilterPolishUtil.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishZ.Economy
{
    public static class ConcreteEnrichmentProcedures
    {
        public static void Initialize()
        {
            EnrichmentProcedureConfiguration.EnrichmentProcedures.AddToMultiple(
                new List<string>() { "uniqueWeapons", "uniqueMaps", "uniqueFlasks", "uniqueArmours", "uniqueAccessory" },
                new List<FilterEconomy.Request.Enrichment.IDataEnrichment>()
                {
                    new LowestPriceEnrichment(),
                    new HighestPriceEnrichment()
                });

            EnrichmentProcedureConfiguration.EnrichmentProcedures.AddToMultiple(
                new List<string>() { "divination", "basetypes" },
                new List<FilterEconomy.Request.Enrichment.IDataEnrichment>()
                {

                });

        }
    }
}
