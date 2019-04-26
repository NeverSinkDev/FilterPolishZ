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
        private FilterEconomyRuleSet uniquemapsRules;
        private FilterEconomyRuleSet fossilrules;
        private FilterEconomyRuleSet shaperRules;
        private FilterEconomyRuleSet elderRules;
        private List<TieringCommand> suggestions;

        public ItemInformationFacade ItemInformation { get; set; }
        public EconomyRequestFacade EconomyInformation { get; set; }
        public TierListFacade TierListFacade { get; set; }
        public List<FilterEconomyRuleSet> Rules { get; set; } = new List<FilterEconomyRuleSet>();
        public ConcreteEconomyRules()
        {
            this.EconomyInformation = EconomyRequestFacade.GetInstance();
            this.ItemInformation = ItemInformationFacade.GetInstance();
            this.TierListFacade = TierListFacade.GetInstance();

            // The list of the suggestion-generating rulesets
            this.uniqueRules = UniqueRulesFactory.Generate(this);
            this.divinationRules = DivinationRuleFactory.Generate(this);
            this.uniquemapsRules = this.GenerateUniqueMapRules();
            this.fossilrules = this.GenerateFossilTieringRules();
            this.shaperRules = ShaperElderRulesFactory.Generate(this,"rare->shaper");
            this.elderRules = ShaperElderRulesFactory.Generate(this,"rare->elder");

            this.Rules.Clear();

            this.Rules.Add(this.uniqueRules);
            this.Rules.Add(this.divinationRules);
            this.Rules.Add(this.uniquemapsRules);
            this.Rules.Add(this.fossilrules);
            this.Rules.Add(this.elderRules);
            this.Rules.Add(this.shaperRules);
        }

        /// <summary>
        /// Generates suggestions, but doesn't apply them.
        /// </summary>
        public void GenerateSuggestions()
        {
            this.Rules.ForEach(x => x.GenerateAndAddSuggestions());
        }

        private FilterEconomyRuleSet GenerateUniqueMapRules()
        {
            return new RuleSetBuilder(this)
                .SetSection("unique->maps")
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget()
                .AddSimpleComparisonRule("t1", "t1", FilterPolishConstants.T1DiviBreakPoint)
                .AddSimpleComparisonRule("t2", "t2", FilterPolishConstants.T2DiviBreakPoint)
                .AddRestRule()
                .Build();
        }

        private FilterEconomyRuleSet GenerateFossilTieringRules()
        {
            return new RuleSetBuilder(this)
                .SetSection("currency->fossil")
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget()
                .AddSimpleComparisonRule("t1", "t1", FilterPolishConstants.T1DiviBreakPoint)
                .AddSimpleComparisonRule("t2", "t2", FilterPolishConstants.T2DiviBreakPoint)
                .AddSimpleReversedComparisonRule("t4", "t4", FilterPolishConstants.T5DiviBreakPoint)
                .AddRestRule()
                .Build();
        }
    }
}
