using FilterEconomy.Facades;
using FilterEconomy.Model;
using FilterPolishUtil.Collections;
using FilterPolishUtil.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilterEconomyProcessor.Enrichment
{
    public class CurrencyInformationExtractionEnrichment : IDataEnrichment
    {
        // TODO: Refactor
        public void Enrich(string baseType, ItemList<NinjaItem> data)
        {
            if (baseType == "Exalted Orb")
            {
                var cPrice = data.Where(x => x.Name == "Exalted Orb")?.FirstOrDefault()?.CVal;

                if (EconomyRequestFacade.GetInstance().IsEarlyLeague() && cPrice > 75)
                {
                    cPrice = 75;
                }

                if (cPrice != null && cPrice > 30)
                {
                    FilterPolishUtil.FilterPolishConfig.ExaltedOrbPrice = cPrice.Value;
                    LoggingFacade.LogInfo($"Exalted Price: {cPrice.Value}");
                }
            }

            return;
        }
    }
}
