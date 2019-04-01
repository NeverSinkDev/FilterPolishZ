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
            this.ItemInformation = ItemInformationFacade.GetInstance();
        }

        public void Execute()
        {
            this.suggestions = EconomyInformation.EconomyTierlistOverview["uniques"].Select(z => z.Key).Select(x => this.uniqueRules.ProcessItem("uniques", x, x, this)).ToList();
        }

        private FilterEconomyRuleSet CreateUniqueEconomyRules()
        {
            FilterEconomyRuleSet uniqueRules = new FilterEconomyRuleSet() { GoverningSection = "uniques" };
            uniqueRules.DefaultItemQuery = new System.Func<string, FilterPolishUtil.Collections.ItemList<FilterEconomy.Model.NinjaItem>>((s) => EconomyInformation.EconomyTierlistOverview["uniques"][s]);

            // Anchor item
            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                TargetTier = "current",
                Rule = (string s) =>
                {
                    return uniqueRules.DefaultSet.Select(x => x.Aspects).ToList()
                    .Any(z => z.Any(a => a.ToString() == "AnchorAspect"));
                }
            });

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
                    return uniqueRules.DefaultSet.LowestPrice > FilterPolishConstants.T1BreakPoint;
                }
            });

            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                TargetTier = "t2",
                Rule = (string s) =>
                {
                    return uniqueRules.DefaultSet.LowestPrice > FilterPolishConstants.T2BreakPoint;
                }
            });

            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                TargetTier = "Potential",
                Rule = (string s) =>
                {
                    return uniqueRules.DefaultSet.HighestPrice > FilterPolishConstants.T2BreakPoint;
                }
            });

            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                TargetTier = "Potential",
                Rule = (string s) =>
                {
                    return uniqueRules.DefaultSet.HighestPrice > FilterPolishConstants.T2BreakPoint;
                }
            });

            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                TargetTier = "Prophecy",
                Rule = (string s) =>
                {
                    var aspects = ItemInformation["uniques", s];
                    if (aspects == null)
                    {
                        return true;
                    }

                    return uniqueRules.DefaultSet.Any(z => z.Aspects.Any(j => j.Name == "ProphecyMaterialAspect"));
                }
            });

            return uniqueRules;
        }
    }
}
