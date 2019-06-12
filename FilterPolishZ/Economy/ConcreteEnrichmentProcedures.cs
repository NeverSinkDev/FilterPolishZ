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
                new List<string>() { "uniques", "unique->maps", "currency->fossil", "currency->incubators" },
                new List<FilterEconomy.Request.Enrichment.IDataEnrichment>()
                {
                    new LowestPriceEnrichment(),
                    new HighestPriceEnrichment()
                });

            EnrichmentProcedureConfiguration.EnrichmentProcedures.AddToMultiple(
                new List<string>() { "divination" },
                new List<FilterEconomy.Request.Enrichment.IDataEnrichment>()
                {
                    new DivinationCardEnrichment(),
                    new LowestPriceEnrichment(),
                });

            EnrichmentProcedureConfiguration.EnrichmentProcedures.AddToMultiple(
                new List<string>() { "rare->shaper", "rare->elder" },
                new List<FilterEconomy.Request.Enrichment.IDataEnrichment>()
                {
                    new ShaperElderEnrichment(),
                });

            EnrichmentProcedureConfiguration.EnrichmentProcedures.AddToMultiple(
                new List<string>() { "basetypes" },
                new List<FilterEconomy.Request.Enrichment.IDataEnrichment>()
                {

                });
        }
    }
}
