using FilterEconomy.Processor;
using FilterPolishUtil;
using System;

namespace FilterEconomyProcessor.RuleSet
{
    public class ProphecyRulesFactory
    {
        public static FilterEconomyRuleSet Generate(ConcreteEconomyRules ruleHost)
        {
            var builder = new RuleSetBuilder(ruleHost)
                .SetSection("currency->prophecy")
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget();

            builder.AddRule("t1", "t1",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.Item.LowestPrice;
                    return price > FilterPolishConfig.DiviT1BreakPoint;
                }));

            builder.AddRule("MultiBase", "t2",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.Item.HighestPrice;
                    return price > FilterPolishConfig.DiviT1BreakPoint;
                }));

            builder.AddRule("t2", "t2",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.Item.LowestPrice;
                    return price > FilterPolishConfig.DiviT2BreakPoint;
                }));

            builder.AddRule("t3", "t3",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.Item.HighestPrice;

                    if (price <= 1f)
                    {
                        return false;
                    }

                    var isDrop = builder.Item.HasAspect("ItemDropProphecyAspect") ? 1.2f : 1f;
                    var isCheap = builder.Item.HasAspect("CheapProphecyAspect") ? 0.5f : 1f;
                    var isUpgrade = builder.Item.HasAspect("ItemUpgradeProphecyAspect") ? 1.2f : 1f;

                    return isUpgrade * price * isDrop * isCheap * 0.5f > FilterPolishConfig.DiviT5BreakPoint;
                }));

            builder.AddRule("t3mapping", "t3mapping",
                new Func<string, bool>((string s) =>
                {
                    return builder.Item.HasAspect("MapUpgradeProphecyAspect");
                }));

            builder.AddRule("t3timeless", "t3",
                new Func<string, bool>((string s) =>
                {
                    return builder.Item.HasAspect("TimelessProphecyAspect") || builder.Item.HasAspect("PreventHidingAspect");
                }));

            builder.AddRule("t4upgrade", "t4upgrade",
                new Func<string, bool>((string s) =>
                {
                    return builder.Item.HasAspect("ItemUpgradeProphecyAspect");
                }));

            builder.AddRule("t4drop", "t4drop",
                new Func<string, bool>((string s) =>
                {
                    return builder.Item.HasAspect("ItemDropProphecyAspect");
                }));

            builder.AddRule("t4", "t4",
                new Func<string, bool>((string s) =>
                {
                    return true;
                }));

            return builder.Build();
        }
    }
}
