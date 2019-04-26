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
        private FilterEconomyRuleSet shaperRules;
        private FilterEconomyRuleSet elderRules;
        private List<TieringCommand> suggestions;

        public ItemInformationFacade ItemInformation { get; set; }
        public EconomyRequestFacade EconomyInformation { get; set; }
        public TierListFacade TierListFacade { get; set; }

        public ConcreteEconomyRules()
        {
            this.EconomyInformation = EconomyRequestFacade.GetInstance();
            this.ItemInformation = ItemInformationFacade.GetInstance();
            this.TierListFacade = TierListFacade.GetInstance();

            this.uniqueRules = UniqueRulesFactory.Generate(this);
            this.divinationRules = DivinationRuleFactory.Generate(this);
            this.uniquemapsRules = this.GenerateUniqueMapRules();
            this.shaperRules = ShaperElderRulesFactory.Generate(this,"rare->shaper");
            this.elderRules = ShaperElderRulesFactory.Generate(this,"rare->elder");
        }

        public void GenerateSuggestions()
        {
            this.uniqueRules.GenerateAndAddSuggestions();
            this.divinationRules.GenerateAndAddSuggestions();
            this.uniquemapsRules.GenerateAndAddSuggestions();
            this.shaperRules.GenerateAndAddSuggestions();
            this.elderRules.GenerateAndAddSuggestions();
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

        private FilterEconomyRuleSet GenerateShaperRules()
        {
            return new RuleSetBuilder(this)
                .SetSection("shaper")
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget()
                .AddSimpleComparisonRule("t1", "t1", FilterPolishConstants.T1DiviBreakPoint)
                .AddSimpleComparisonRule("t2", "t2", FilterPolishConstants.T2DiviBreakPoint)
                .AddRestRule()
                .Build();
        }
    }
}
