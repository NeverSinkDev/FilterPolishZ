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

            builder.AddRule("ANCHOR", "ANCHOR",
                new Func<string, bool>((string s) =>
                {
                    return builder.RuleSet.DefaultSet.HasAspect("AnchorAspect");
                }));

            builder.AddRule("t1", "t1",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.RuleSet.DefaultSet.LowestPrice;
                    return price > FilterPolishConfig.T1DiviBreakPoint;
                }));

            builder.AddRule("MultiBase", "t2",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.RuleSet.DefaultSet.HighestPrice;
                    return price > FilterPolishConfig.T1DiviBreakPoint;
                }));

            builder.AddRule("t2", "t2",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.RuleSet.DefaultSet.LowestPrice;
                    return price > FilterPolishConfig.T2DiviBreakPoint;
                }));

            builder.AddRule("rest", "rest",
                new Func<string, bool>((string s) =>
                {
                    return true;
                }));

            return builder.Build();
        }
    }
}
