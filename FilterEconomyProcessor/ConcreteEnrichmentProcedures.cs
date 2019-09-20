using FilterEconomy.Model;
using FilterEconomy.Request;
using FilterEconomyProcessor.Enrichment;
using FilterPolishUtil.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterEconomyProcessor
{
    public static class EnrichmentProcedureConfiguration
    {
        public static Dictionary<string, List<IDataEnrichment>> EnrichmentProcedures = new Dictionary<string, List<IDataEnrichment>>();
    }

    public static class ConcreteEnrichmentProcedures
    {
        public static void Initialize()
        {
            EnrichmentProcedureConfiguration.EnrichmentProcedures.AddToMultiple(
                new List<string>() { "uniques", "unique->maps", "currency->fossil", "currency->incubators", "currency->prophecy", "fragments->scarabs", "currency->oil" },
                new List<IDataEnrichment>()
                {
                    new LowestPriceEnrichment(),
                    new HighestPriceEnrichment()
                });

            EnrichmentProcedureConfiguration.EnrichmentProcedures.AddToMultiple(
                new List<string>() { "divination" },
                new List<IDataEnrichment>()
                {
                    new DivinationCardEnrichment(),
                    new LowestPriceEnrichment(),
                });

            EnrichmentProcedureConfiguration.EnrichmentProcedures.AddToMultiple(
                new List<string>() { "rare->shaper", "rare->elder", "generalcrafting" },
                new List<IDataEnrichment>()
                {
                    new ShaperElderEnrichment(),
                });

            EnrichmentProcedureConfiguration.EnrichmentProcedures.AddToMultiple(
                new List<string>() { "basetypes" },
                new List<IDataEnrichment>()
                {

                });
        }
    }
}
