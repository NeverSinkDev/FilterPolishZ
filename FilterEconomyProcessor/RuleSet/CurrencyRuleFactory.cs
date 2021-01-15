using FilterEconomy.Processor;
using FilterPolishUtil;
using System;

namespace FilterEconomyProcessor.RuleSet
{
    public class CurrencyRuleFactory
    {
        public static FilterEconomyRuleSet Generate(ConcreteEconomyRules ruleHost)
        {
            var builder = new RuleSetBuilder(ruleHost)
                .SetSection("currency")
                // .SkipInEarlyLeague()
                // .LimitExecutionMode(ExecutionMode.Function)
                .UseDefaultQuery()
                // .OverrideMinimalExaltedPriceThreshhold(40)
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

            builder.AddSimpleComparisonRule("ExTier", "t11", Math.Max(20,(FilterPolishConfig.ExaltedOrbPrice / 2f)));
            builder.AddSimpleComparisonRule("DivineTier", "t12", Math.Max(7,(FilterPolishConfig.ExaltedOrbPrice / 5f)));
            builder.AddSimpleComparisonRule("SextantTier", "t21", 1.5f);

            // chaos level rules
            builder.AddSimpleComparisonRule("ChaosTier", "t22", 0.5f);
            builder.AddSimpleAspectContainerRule("ChaosBottom", "t22", "ChaosBottomAspect");

            // alchemy level rules
            builder.AddSimpleComparisonRule("AlchemyTier", "t23", 0.20f);
            builder.AddSimpleAspectContainerRule("AlchemyBottom", "t23", "AlchemyBottomAspect");
            
            builder.AddRule("t32", x =>
            {
                return (builder.Item.HasAspect("EarlyLeagueInterestAspect") &&
                        builder.Item.HasAspect("PoorDropAspect"));
            });

            builder.AddEarlyLeagueHandling("t23");

            builder.AddSimpleComparisonRule("SilverAltTier", "t31", 0.12f);
            builder.AddSimpleAspectContainerRule("SilverBottom", "t31", "SilverBottomAspect");

            builder.AddSimpleComparisonRule("ChanceTier", "t32", 0.05f);
            builder.AddSimpleAspectContainerRule("ChanceBottom", "t32", "ChanceBottomAspect");

            builder.AddExplicitRest("TransmuteTier", "t33");

            return builder.Build();
        }
    }
}
