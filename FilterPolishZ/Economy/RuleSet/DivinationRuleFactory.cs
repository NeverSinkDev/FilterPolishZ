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
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget();


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
                    var price = builder.RuleSet.DefaultSet.LowestPrice * builder.RuleSet.DefaultSet.ValueMultiplier;
                    return price > FilterPolishConstants.T1DiviBreakPoint;
                }));


            builder.AddRule("t2", "t2",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.RuleSet.DefaultSet.LowestPrice * builder.RuleSet.DefaultSet.ValueMultiplier;
                    return price > FilterPolishConstants.T2DiviBreakPoint;
                }));

            builder.AddRule("SingleCardSave", "t2",
                new Func<string, bool>((string s) =>
                {
                    return builder.RuleSet.DefaultSet.HasAspect("SingleCardAspect");
                }));

            builder.AddRule("t3", "t3",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.RuleSet.DefaultSet.LowestPrice * builder.RuleSet.DefaultSet.ValueMultiplier;
                    return price > FilterPolishConstants.T3DiviBreakPoint;
                }));

            builder.AddRule("TimelessSave", "t3",
                new Func<string, bool>((string s) =>
                {
                    return builder.RuleSet.DefaultSet.HasAspect("TimelessResultAspect");
                }));

            builder.AddRule("CurrencySaveT4", "t4c",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.RuleSet.DefaultSet.LowestPrice * builder.RuleSet.DefaultSet.ValueMultiplier;
                    return (price > FilterPolishConstants.T5DiviBreakPoint) && builder.RuleSet.DefaultSet.HasAspect("CurrencyTypeAspect");
                }));

            builder.AddRule("CurrencySaveT4X", "t4c",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.RuleSet.DefaultSet.LowestPrice * builder.RuleSet.DefaultSet.ValueMultiplier;
                    return builder.RuleSet.DefaultSet.HasAspect("FarmableOrbAspect");
                }));

            builder.AddRule("RandomSave", "rest",
                new Func<string, bool>((string s) =>
                {
                    return builder.RuleSet.DefaultSet.HasAspect("LargeRandomPoolAspect");
                }));

            builder.AddRule("CurrencySaveT5", "t5c",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.RuleSet.DefaultSet.LowestPrice * builder.RuleSet.DefaultSet.ValueMultiplier;
                    return (price < FilterPolishConstants.T5DiviBreakPoint) && builder.RuleSet.DefaultSet.HasAspect("CurrencyTypeAspect");
                }));

            builder.AddRule("t5", "t5",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.RuleSet.DefaultSet.LowestPrice * builder.RuleSet.DefaultSet.ValueMultiplier;
                    return price < FilterPolishConstants.T5DiviBreakPoint;
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
