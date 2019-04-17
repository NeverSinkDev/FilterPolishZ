using FilterEconomy.Processor;
using FilterPolishUtil.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishZ.Economy.RuleSet
{
    public static class DivinationRuleFactory
    {
        public static FilterEconomyRuleSet Generate(ConcreteEconomyRules ruleHost)
        {
            var builder = new RuleSetBuilder(ruleHost)
                .SetSection("divination")
                .UseDefaultQuery()
                .AddDefaultPreprocessing();


            builder.AddRule("ANCHOR", "ANCHOR",
                new Func<string, bool>((string s) =>
                {
                    return builder.RuleSet.DefaultSet.Select(x => x.Aspects).ToList()
                            .Any(z => z.Any(a => a.ToString() == "AnchorAspect"));
                }));


            builder.AddRule("unknown", "unknown",
                new Func<string, bool>((string s) =>
                {
                    return !ruleHost.EconomyInformation.EconomyTierlistOverview["divination"].ContainsKey(s);
                }));


            builder.AddRule("t1", "t1",
                new Func<string, bool>((string s) =>
                {
                    return builder.RuleSet.DefaultSet.LowestPrice > FilterPolishConstants.T1DiviBreakPoint;
                }));


            builder.AddRule("t2", "t2",
                new Func<string, bool>((string s) =>
                {
                    return builder.RuleSet.DefaultSet.LowestPrice > FilterPolishConstants.T2DiviBreakPoint;
                }));


            builder.AddRule("t3", "t3",
                new Func<string, bool>((string s) =>
                {
                    return builder.RuleSet.DefaultSet.LowestPrice > FilterPolishConstants.T3DiviBreakPoint;
                }));


            builder.AddRule("t5", "t5",
                new Func<string, bool>((string s) =>
                {
                    return builder.RuleSet.DefaultSet.LowestPrice < FilterPolishConstants.T5DiviBreakPoint;
                }));


            builder.AddRule("savedRandom", "rest",
                new Func<string, bool>((string s) =>
                {
                    return true;
                }));


            builder.AddRule("savedRandom", "rest",
                new Func<string, bool>((string s) =>
                {
                    return true;
                }));


            builder.AddRule("savedRandom", "rest",
                new Func<string, bool>((string s) =>
                {
                    return true;
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
