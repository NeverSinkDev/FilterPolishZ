using FilterEconomy.Processor;
using FilterPolishUtil;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilterEconomyProcessor.RuleSet
{
    public class CurrencyRuleFactory
    {
        public static FilterEconomyRuleSet Generate(ConcreteEconomyRules ruleHost)
        {
            var builder = new RuleSetBuilder(ruleHost)
                .SetSection("currency")
                .SkipInEarlyLeague()
                .LimitExecutionMode(ExecutionMode.Function)
                .UseDefaultQuery()
                .OverrideMinimalExaltedPriceThreshhold(40)
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget();

            builder.AddRule("No Tiering", "???",
                new Func<string, bool>((string s) =>
                {
                    var isTierable = builder.Item.HasAspect("TierableCurrencyAspect");
                    if (isTierable)
                    {
                        return false;
                    }

                    return true;
                }));

            builder.AddSimpleComparisonRule("ExTier", "t11", FilterPolishConfig.ExaltedOrbPrice / 2);
            builder.AddSimpleComparisonRule("DivineTier", "t12", FilterPolishConfig.ExaltedOrbPrice / 5);
            builder.AddSimpleComparisonRule("SextantTier", "t21", 2f);
            builder.AddSimpleComparisonRule("ChaosTier", "t22", 0.75f);
            builder.AddSimpleAspectContainerRule("ChaosBottom", "t22", "ChaosBottomAspect");
            builder.AddSimpleComparisonRule("AlchemyTier", "t23", 0.20f);
            builder.AddSimpleAspectContainerRule("AlchemyBottom", "t23", "AlchemyButtomAspect");
            builder.AddSimpleComparisonRule("SilverTier", "t31", 0.15f);
            builder.AddSimpleAspectContainerRule("SilverBottom", "t31", "SilverBottomAspect");
            builder.AddSimpleComparisonRule("ChanceTier", "t32", 0.04f);
            builder.AddSimpleAspectContainerRule("ChanceBottom", "t32", "ChanceBottomAspect");

            builder.AddExplicitRest("TransmuteTier", "t33");

            return builder.Build();
        }
    }
}
