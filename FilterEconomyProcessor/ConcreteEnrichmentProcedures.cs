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
        public static Dictionary<string, List<IDataEnrichment>> PriorityEnrichmentProcedures = new Dictionary<string, List<IDataEnrichment>>();
        public static Dictionary<string, List<IDataEnrichment>> EnrichmentProcedures = new Dictionary<string, List<IDataEnrichment>>();
    }

    public static class ConcreteEnrichmentProcedures
    {
        public static void Initialize()
        {
            EnrichmentProcedureConfiguration.PriorityEnrichmentProcedures.Clear();
            EnrichmentProcedureConfiguration.EnrichmentProcedures.Clear();

            // Ugly implementation, but needs to be implemented first, since we're relying on the 
            EnrichmentProcedureConfiguration.PriorityEnrichmentProcedures.AddToMultiple(
                new List<string>() { "currency" },
                new List<IDataEnrichment>()
                {
                    new LowestPriceEnrichment(),
                    new CurrencyInformationExtractionEnrichment()
                });

            // Now perform all the 
            EnrichmentProcedureConfiguration.EnrichmentProcedures.AddToMultiple(
                new List<string>() { "uniques", "fragments" , "unique->maps", "currency->fossil", "currency->incubators", "currency->deliriumorbs", "currency->prophecy", "fragments->scarabs", "currency->oil", "vials" },
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
                new List<string>() { "rare->shaper", "rare->elder", "rare->warlord", "rare->hunter", "rare->crusader", "rare->redeemer", "generalcrafting" },
                new List<IDataEnrichment>()
                {
                    new ShaperElderEnrichment(),
                    new LowestPriceEnrichment()
                });

            EnrichmentProcedureConfiguration.EnrichmentProcedures.AddToMultiple(
                new List<string>() { "basetypes" },
                new List<IDataEnrichment>()
                {

                });
        }
    }
}
