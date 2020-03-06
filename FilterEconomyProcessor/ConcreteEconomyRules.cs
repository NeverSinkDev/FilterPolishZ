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
        private FilterEconomyRuleSet prophecyRules;
        private FilterEconomyRuleSet scarabRules;
        private FilterEconomyRuleSet normalRules;
        private FilterEconomyRuleSet oilRules;
        private FilterEconomyRuleSet vialRules;

        private FilterEconomyRuleSet fragmentRules;
        private FilterEconomyRuleSet currencyRules;

        private FilterEconomyRuleSet shaperRules;
        private FilterEconomyRuleSet elderRules;
        private FilterEconomyRuleSet crusaderRules;
        private FilterEconomyRuleSet hunterRules;
        private FilterEconomyRuleSet redeemerRules;
        private FilterEconomyRuleSet warlordRules;

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
            this.fragmentRules = FragmentsRuleFactory.Generate(this);
            this.currencyRules = CurrencyRuleFactory.Generate(this);

            this.scarabRules = this.GenerateScarabRuleSet();
            this.uniquemapsRules = this.GenerateUniqueMapRules();
            this.fossilrules = this.GenerateFossilTieringRules();
            this.incubatorrules = this.GenerateIncubatorTieringRules();
            this.oilRules = this.GenerateBlightOilRuleSet();
            this.vialRules = this.GenerateVialRuleSet();

            this.shaperRules = ShaperElderRulesFactory.Generate(this,"rare->shaper");
            this.elderRules = ShaperElderRulesFactory.Generate(this,"rare->elder");
            this.hunterRules = ShaperElderRulesFactory.Generate(this,"rare->hunter");
            this.warlordRules = ShaperElderRulesFactory.Generate(this,"rare->warlord");
            this.crusaderRules = ShaperElderRulesFactory.Generate(this,"rare->crusader");
            this.redeemerRules = ShaperElderRulesFactory.Generate(this,"rare->redeemer");

            this.normalRules = NormalCraftingBasesRuleFactory.Generate(this, "generalcrafting");

            this.Rules.Clear();

            this.Rules.Add(this.uniqueRules);
            this.Rules.Add(this.divinationRules);
            this.Rules.Add(this.uniquemapsRules);
            this.Rules.Add(this.fossilrules);
            this.Rules.Add(this.fragmentRules);
            this.Rules.Add(this.currencyRules);
            // this.Rules.Add(this.vialRules);

            this.Rules.Add(this.incubatorrules);
            this.Rules.Add(this.prophecyRules);
            this.Rules.Add(this.scarabRules);
            this.Rules.Add(this.oilRules);
            this.Rules.Add(this.normalRules);

            this.Rules.Add(this.elderRules);
            this.Rules.Add(this.shaperRules);

            this.Rules.Add(this.hunterRules);
            this.Rules.Add(this.redeemerRules);
            this.Rules.Add(this.crusaderRules);
            this.Rules.Add(this.warlordRules);
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
                .AddSimpleComparisonRule("t2", "t2", FilterPolishConfig.UniqueT2BreakPoint)
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
                .AddSimpleComparisonRule("t1", "t1", FilterPolishConfig.MiscT1BreakPoint)
                .AddSimpleComparisonRule("t2", "t2", FilterPolishConfig.MiscT2BreakPoint)
                .AddExplicitRest("t3", "t3")
                .Build();
        }

        private FilterEconomyRuleSet GenerateFossilTieringRules()
        {
            return new RuleSetBuilder(this)
                .SetSection("currency->fossil")
                .OverrideMinimalExaltedPriceThreshhold(50)
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
