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
        private FilterEconomyRuleSet uniqueRules;
        private FilterEconomyRuleSet divinationRules;
        private FilterEconomyRuleSet uniquemapsRules;
        private FilterEconomyRuleSet fossilrules;
        private FilterEconomyRuleSet incubatorrules;
        private FilterEconomyRuleSet shaperRules;
        private FilterEconomyRuleSet elderRules;
        private FilterEconomyRuleSet prophecyRules;
        private FilterEconomyRuleSet scarabRules;
        private FilterEconomyRuleSet normalRules;
        private FilterEconomyRuleSet oilRules;
        private FilterEconomyRuleSet fragmentRules;


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
            this.prophecyRules = ProphecyRulesFactory.Generate(this);

            this.scarabRules = this.GenerateScarabRuleSet();
            this.uniquemapsRules = this.GenerateUniqueMapRules();
            this.fossilrules = this.GenerateFossilTieringRules();
            this.incubatorrules = this.GenerateIncubatorTieringRules();
            this.oilRules = this.GenerateBlightOilRuleSet();
            this.fragmentRules = this.GenerateFragmentRules();

            this.shaperRules = ShaperElderRulesFactory.Generate(this,"rare->shaper");
            this.elderRules = ShaperElderRulesFactory.Generate(this,"rare->elder");
            this.normalRules = NormalCraftingBasesRuleFactory.Generate(this, "generalcrafting");

            this.Rules.Clear();

            this.Rules.Add(this.uniqueRules);
            this.Rules.Add(this.divinationRules);
            this.Rules.Add(this.uniquemapsRules);
            this.Rules.Add(this.fossilrules);
            this.Rules.Add(this.elderRules);
            this.Rules.Add(this.shaperRules);
            this.Rules.Add(this.incubatorrules);
            this.Rules.Add(this.prophecyRules);
            this.Rules.Add(this.scarabRules);
            this.Rules.Add(this.oilRules);
            this.Rules.Add(this.normalRules);
            //this.Rules.Add(this.fragmentRules);
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
                .OverrideMinimalExaltedPriceThreshhold(45)
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget()
                .AddSimpleComparisonRule("t1", "t1", FilterPolishConfig.UniqueT1BreakPoint)
                .AddSimpleComparisonRule("t2", "t2", FilterPolishConfig.UniqueT2BreakPoint)
                .AddRestRule()
                .Build();       
        }

        private FilterEconomyRuleSet GenerateFragmentRules()
        {
            return new RuleSetBuilder(this)
                .SetSection("fragments")
                .OverrideMinimalExaltedPriceThreshhold(50)
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget()
                .AddSimpleComparisonRule("t1", "t1", FilterPolishConfig.MiscT1BreakPoint)
                .AddSimpleComparisonRule("t2", "t2", FilterPolishConfig.MiscT2BreakPoint)
                .AddRestRule()
                .Build();
        }

        private FilterEconomyRuleSet GenerateIncubatorTieringRules()
        {
            return new RuleSetBuilder(this)
                .SetSection("currency->incubators")
                .OverrideMinimalExaltedPriceThreshhold(45)
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget()
                .AddSimpleComparisonRule("t1", "t1", FilterPolishConfig.DiviT1BreakPoint)
                .AddSimpleComparisonRule("t2", "t2", FilterPolishConfig.DiviT2BreakPoint)
                .AddRestRule()
                .Build();
        }

        private FilterEconomyRuleSet GenerateScarabRuleSet()
        {
            return new RuleSetBuilder(this)
                .SetSection("fragments->scarabs")
                .OverrideMinimalExaltedPriceThreshhold(45)
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget()
                .AddSimpleComparisonRule("t1", "t1", FilterPolishConfig.MiscT1BreakPoint)
                .AddSimpleComparisonRule("t2", "t2", FilterPolishConfig.MiscT2BreakPoint)
                .AddRestRule()
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
                .AddRestRule()
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
                .AddSimpleReversedComparisonRule("t4", "t4", FilterPolishConfig.MiscT4BreakPoint * 0.5f) // to prevent from getting most oils into the trash tier.
                .AddExplicitRest("t3","t3")
                .Build();
        }
    }
}
