using FilterCore.FilterComponents.Tier;
using FilterEconomy.Facades;
using FilterEconomy.Processor;
using FilterUtilModels.Economy;
using System.Collections.Generic;

namespace FilterPolishZ.Economy
{
    public class ConcreteEconomyRules : IEconomyProcessorData
    {
        public ItemInformationFacade ItemInformation { get; set; }
        public EconomyRequestFacade EconomyInformation { get; set; }
        public Dictionary<string, TierGroup> TierInformation { get; set; }

        public ConcreteEconomyRules()
        {
            this.CreateUniqueEconomyRules();
        }

        private void CreateUniqueEconomyRules()
        {
            FilterEconomyRuleSet uniqueRules = new FilterEconomyRuleSet() { GoverningSection = "uniques" };
            uniqueRules.EconomyRules.Add(new FilterEconomyRule() {
                TargetTier = "T1",
                Rule = (string s) =>  s.Length > 0 });
        }
    }
}
