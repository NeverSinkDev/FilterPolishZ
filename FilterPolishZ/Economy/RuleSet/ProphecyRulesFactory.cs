using FilterEconomy.Processor;
using FilterPolishUtil.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishZ.Economy.RuleSet
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
                    return price > FilterPolishConstants.T1DiviBreakPoint;
                }));

            builder.AddRule("MultiBase", "t1",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.RuleSet.DefaultSet.HighestPrice;
                    return price > FilterPolishConstants.T2DiviBreakPoint;
                }));

            builder.AddRule("t2", "t2",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.RuleSet.DefaultSet.LowestPrice;
                    return price > FilterPolishConstants.T2DiviBreakPoint;
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
