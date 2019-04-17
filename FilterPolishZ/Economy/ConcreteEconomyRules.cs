using FilterCore.FilterComponents.Tier;
using FilterEconomy;
using FilterEconomy.Facades;
using FilterEconomy.Model;
using FilterEconomy.Processor;
using FilterPolishUtil;
using FilterPolishUtil.Constants;
using FilterPolishZ.Economy.RuleSet;
using FilterUtilModels.Economy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FilterPolishZ.Economy
{
    public class ConcreteEconomyRules : IEconomyProcessorData
    {
        private FilterEconomyRuleSet uniqueRules;
        private FilterEconomyRuleSet divinationRules;

        private List<TieringCommand> suggestions;

        public ItemInformationFacade ItemInformation { get; set; }
        public EconomyRequestFacade EconomyInformation { get; set; }
        public Dictionary<string, TierGroup> TierInformation { get; set; }
        public TierListFacade TierListFacade { get; set; }

        public ConcreteEconomyRules()
        {
            this.uniqueRules = UniqueRulesFactory.Generate(this);
            this.divinationRules = DivinationRuleFactory.Generate(this);

            this.EconomyInformation = EconomyRequestFacade.GetInstance();
            this.ItemInformation = ItemInformationFacade.GetInstance();
            this.TierListFacade = TierListFacade.GetInstance();
        }


        public void Execute()
        {
            TierListFacade.Suggestions["uniques"].Clear();
            TierListFacade.Suggestions["uniques"].AddRange(this.uniqueRules.GenerateSuggestions());
            TierListFacade.Suggestions["divination"].Clear();
            TierListFacade.Suggestions["divination"].AddRange(this.divinationRules.GenerateSuggestions());
            // PerformUniqueActions();
            // PerfromDivinationCardActions();
        }

        private void PerfromDivinationCardActions()
        {
            this.suggestions = EconomyInformation.EconomyTierlistOverview["divination"].Select(z => z.Key).Select(x => this.divinationRules.ProcessItem("divination", x, x, this)).ToList();

            foreach (var item in suggestions)
            {
                item.Group = "divination";

                if (TierListFacade.ContainsTierInformationForBaseType("divination", item.BaseType))
                {
                    item.OldTier = TierListFacade.GetTiersForBasetype("divination", item.BaseType).First().SubStringLast("->");
                }
                else
                {
                    item.OldTier = "rest";
                }
            }

            this.TierListFacade.Suggestions["divination"].AddRange(this.suggestions);
        }

        private FilterEconomyRuleSet CreateDivinationRules()
        {
            FilterEconomyRuleSet diviRules = new FilterEconomyRuleSet() { GoverningSection = "divination" };
            diviRules.DefaultItemQuery = new System.Func<string, FilterPolishUtil.Collections.ItemList<FilterEconomy.Model.NinjaItem>>((s) => EconomyInformation.EconomyTierlistOverview["divination"][s]);

            diviRules.EconomyRules.Add(new FilterEconomyRule()
            {
                RuleName = "rest",
                TargetTier = "rest",
                Rule = (string s) =>
                {
                    return true;
                }
            });

            return diviRules;
        }
    }
}
