using FilterEconomy.Facades;
using FilterEconomy.Processor;
using FilterEconomyProcessor.RuleSet;
using FilterPolishUtil;
using FilterPolishUtil.Model;
using FilterPolishZ.Economy.RuleSet;
using FilterUtilModels.Economy;
using System;
using System.Collections.Generic;

namespace FilterEconomyProcessor
{
    public class ConcreteEconomyRules : IEconomyProcessorData
    {
        public ItemInformationFacade ItemInformation { get; set; }
        public EconomyRequestFacade EconomyInformation { get; set; }
        public TierListFacade TierListFacade { get; set; }
        public List<FilterEconomyRuleSet> Rules { get; set; } = new List<FilterEconomyRuleSet>();
        public ConcreteEconomyRules()
        {
            this.EconomyInformation = EconomyRequestFacade.GetInstance();
            this.ItemInformation = ItemInformationFacade.GetInstance();
            this.TierListFacade = TierListFacade.GetInstance();

            this.Rules.Clear();

            // The list of the suggestion-generating rulesets
            this.Rules.Add(UniqueRulesFactory.Generate(this));
            this.Rules.Add(DivinationRuleFactory.Generate(this));
            this.Rules.Add(ProphecyRulesFactory.Generate(this));
            this.Rules.Add(FragmentsRuleFactory.Generate(this));
            this.Rules.Add(CurrencyRuleFactory.Generate(this));

            this.Rules.Add(this.GenerateScarabRuleSet());
            this.Rules.Add(this.GenerateUniqueMapRules());
            this.Rules.Add(this.GenerateFossilTieringRules());
            this.Rules.Add(this.GenerateIncubatorTieringRules());
            this.Rules.Add(this.GenerateBlightOilRuleSet());
            this.Rules.Add(this.GenerateVialRuleSet());
            this.Rules.Add(this.GenerateDeliriumorbsRuleSet());

            this.Rules.Add(ShaperElderRulesFactory.Generate(this,"rare->shaper"));
            this.Rules.Add(ShaperElderRulesFactory.Generate(this,"rare->elder"));
            this.Rules.Add(ShaperElderRulesFactory.Generate(this,"rare->hunter"));
            this.Rules.Add(ShaperElderRulesFactory.Generate(this,"rare->warlord"));
            this.Rules.Add(ShaperElderRulesFactory.Generate(this,"rare->crusader"));
            this.Rules.Add(ShaperElderRulesFactory.Generate(this,"rare->redeemer"));

            this.Rules.Add(NormalCraftingBasesRuleFactory.Generate(this, "generalcrafting"));
        }

        /// <summary>
        /// Generates suggestions, but doesn't apply them.
        /// </summary>
        public void GenerateSuggestions()
        {
            this.Rules.ForEach(x => x.GenerateAndAddSuggestions());
            LoggingFacade.LogInfo("Done (Re)generating Tiering Suggestions");
        }

        private FilterEconomyRuleSet GenerateUniqueMapRules()
        {
            return new RuleSetBuilder(this)
                .SetSection("unique->maps")
                .OverrideMinimalExaltedPriceThreshhold(50)
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget()
                .AddSimpleComparisonRule("t1", "t1", FilterPolishConfig.UniqueT1BreakPoint)
                .AddRawAverageComparison("t2", "t2", FilterPolishConfig.UniqueT2BreakPoint)
                .AddRawAverageComparison("t3safe", "t3", FilterPolishConfig.UniqueT2BreakPoint / 2)
                .AddPoorDropRoutine("t4", FilterPolishConfig.MiscT4BreakPoint * 2, 3f, pricingMode: PricingMode.lowest)
                .AddExplicitRest("t3","t3")
                .Build();       
        }

        private FilterEconomyRuleSet GenerateIncubatorTieringRules()
        {
            return new RuleSetBuilder(this)
                .SetSection("currency->incubators")
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget()
                .AddSimpleComparisonRule("t1", "t1", FilterPolishConfig.DiviT1BreakPoint)
                .AddSimpleComparisonRule("t2", "t2", FilterPolishConfig.DiviT2BreakPoint)
                .AddPoorDropRoutine("t4", FilterPolishConfig.DiviT5BreakPoint, 3.5f)
                .AddExplicitRest("t3", "t3")
                .Build();
        }

        private FilterEconomyRuleSet GenerateScarabRuleSet()
        {
            return new RuleSetBuilder(this)
                .SetSection("fragments->scarabs")
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget()
                .SkipInEarlyLeague()
                .AddSimpleComparisonRule("t1", "t1", FilterPolishConfig.MiscT1BreakPoint)
                .AddSimpleComparisonRule("t2", "t2", FilterPolishConfig.MiscT2BreakPoint)
                .AddPoorDropRoutine("t4", FilterPolishConfig.MiscT4BreakPoint)
                .AddExplicitRest("t3", "t3")
                .Build();
        }

        private FilterEconomyRuleSet GenerateDeliriumorbsRuleSet()
        {
            return new RuleSetBuilder(this)
                .SetSection("currency->deliriumorbs")
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget()
                .SkipInEarlyLeague()
                .AddSimpleComparisonRule("t1", "t1", FilterPolishConfig.MiscT1BreakPoint)
                .AddSimpleComparisonRule("t2", "t2", FilterPolishConfig.MiscT2BreakPoint)
                .AddExplicitRest("t3", "t3")
                .Build();
        }

        private FilterEconomyRuleSet GenerateFossilTieringRules()
        {
            return new RuleSetBuilder(this)
                .SetSection("currency->fossil")
                .OverrideMinimalExaltedPriceThreshhold(45)
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget()
                .AddSimpleComparisonRule("t1", "t1", FilterPolishConfig.MiscT1BreakPoint)
                .AddSimpleComparisonRule("t2", "t2", FilterPolishConfig.MiscT2BreakPoint)
                .AddSimpleReversedComparisonRule("t4", "t4", FilterPolishConfig.MiscT4BreakPoint)
                .AddExplicitRest("t3", "t3")
                .Build();
        }

        private FilterEconomyRuleSet GenerateBlightOilRuleSet()
        {
            return new RuleSetBuilder(this)
                .SetSection("currency->oil")
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget()
                .AddSimpleComparisonRule("t1", "t1", FilterPolishConfig.MiscT1BreakPoint)
                .AddSimpleComparisonRule("t2", "t2", FilterPolishConfig.MiscT2BreakPoint)
                .AddSimpleReversedComparisonRule("t4", "t4", FilterPolishConfig.MiscT4BreakPoint)
                .AddExplicitRest("t3","t3")
                .Build();
        }

        private FilterEconomyRuleSet GenerateVialRuleSet()
        {
            return new RuleSetBuilder(this)
                .SetSection("vials")
                .OverrideMinimalExaltedPriceThreshhold(40)
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget()
                .AddSimpleComparisonRule("t1", "t1", FilterPolishConfig.MiscT1BreakPoint)
                .AddSimpleComparisonRule("t2", "t2", FilterPolishConfig.MiscT2BreakPoint)
                .AddExplicitRest("t3", "t3")
                .Build();
        }
    }
}
