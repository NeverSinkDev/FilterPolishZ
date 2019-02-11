using FilterCore.FilterComponents.Tier;
using FilterEconomy.Facades;
using FilterEconomy.Processor;
using FilterPolishUtil.Constants;
using FilterUtilModels.Economy;
using System.Collections.Generic;
using System.Linq;

namespace FilterPolishZ.Economy
{
    public class ConcreteEconomyRules : IEconomyProcessorData
    {
        private FilterEconomyRuleSet uniqueRules;
        private List<TieringCommand> suggestions;

        public ItemInformationFacade ItemInformation { get; set; }
        public EconomyRequestFacade EconomyInformation { get; set; }
        public Dictionary<string, TierGroup> TierInformation { get; set; }

        public ConcreteEconomyRules()
        {
            this.uniqueRules = this.CreateUniqueEconomyRules();
        }

        public void Execute()
        {
            this.suggestions = EconomyInformation.EconomyTierlistOverview["uniques"].Select(z => z.Key).Select(x => this.uniqueRules.ProcessItem("uniques", x, this)).ToList();
        }

        private FilterEconomyRuleSet CreateUniqueEconomyRules()
        {
            FilterEconomyRuleSet uniqueRules = new FilterEconomyRuleSet() { GoverningSection = "uniques" };

            List<string> list = new List<string>();

            // Unknown Unique
            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                TargetTier = "unknown",

                Rule = (string s) =>
                {
                    return !EconomyInformation.EconomyTierlistOverview["uniques"].ContainsKey(s);
                }
            });

            // T1 unique
            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                TargetTier = "t1",
                Rule = (string s) =>
                {
                    var items = EconomyInformation.EconomyTierlistOverview["uniques"][s].ToList();
                    return items.All(x => x.CVal > FilterPolishConstants.T1BreakPoint);
                }
            });

            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                TargetTier = "t2",
                Rule = (string s) =>
                {
                    var items = EconomyInformation.EconomyTierlistOverview["uniques"][s].ToList();
                    return items.All(x => x.CVal > FilterPolishConstants.T2BreakPoint);
                }
            });

            return uniqueRules;
        }
    }
}
