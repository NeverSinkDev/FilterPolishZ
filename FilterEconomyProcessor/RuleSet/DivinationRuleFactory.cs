using FilterEconomy.Processor;
using FilterEconomyProcessor;
using FilterEconomyProcessor.RuleSet;
using FilterPolishUtil;
using System;

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
                    return builder.RuleSet.DefaultSet.HasAspect("AnchorAspect");
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
                    return price > FilterPolishConfig.DiviT1BreakPoint;
                }));


            builder.AddRule("t2", "t2",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.RuleSet.DefaultSet.LowestPrice * builder.RuleSet.DefaultSet.ValueMultiplier;
                    return price > FilterPolishConfig.DiviT2BreakPoint;
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
                    return price > FilterPolishConfig.DiviT3BreakPoint;
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

                    if (builder.RuleSet.DefaultSet.HasAspect("CurrencyTypeAspect"))
                    {
                        if (builder.RuleSet.DefaultSet.HasAspect("PoorDiviAspect"))
                        {
                            return (price > FilterPolishConfig.DiviT5BreakPoint * 2);
                        }
                        else
                        {
                            return (price > FilterPolishConfig.DiviT5BreakPoint);
                        }
                    }

                    return false;
                }));

            builder.AddRule("CurrencySaveT4X", "t4c",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.RuleSet.DefaultSet.LowestPrice * builder.RuleSet.DefaultSet.ValueMultiplier;
                    return builder.RuleSet.DefaultSet.HasAspect("FarmableOrbAspect");
                }));

            builder.AddRule("CurrencySaveT5", "t5c",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.RuleSet.DefaultSet.LowestPrice * builder.RuleSet.DefaultSet.ValueMultiplier;
                    return (price < FilterPolishConfig.DiviT5BreakPoint || builder.RuleSet.DefaultSet.HasAspect("PoorDiviAspect")) && builder.RuleSet.DefaultSet.HasAspect("CurrencyTypeAspect");
                }));

            builder.AddRule("RandomSave", "rest",
                new Func<string, bool>((string s) =>
                {
                    return builder.RuleSet.DefaultSet.HasAspect("LargeRandomPoolAspect");
                }));


            builder.AddRule("PoorCard", "t5",
                new Func<string, bool>((string s) =>
                {
                    var price = builder.RuleSet.DefaultSet.LowestPrice * builder.RuleSet.DefaultSet.ValueMultiplier;
                    return (price < (FilterPolishConfig.DiviT5BreakPoint * 2f) && builder.RuleSet.DefaultSet.HasAspect("PoorDiviAspect"));
                }));

            builder.AddRule("T5RedemptionPrevented", "t5",
                new Func<string, bool>((string s) =>
                {
                    if (builder.GetTierOfItem(s).Contains("t5"))
                    {
                        var price = builder.RuleSet.DefaultSet.LowestPrice * builder.RuleSet.DefaultSet.ValueMultiplier;
                        return price < FilterPolishConfig.DiviT5BreakPoint * 1.5;
                    }

                    return false;
                }));

            builder.AddRule("t5", "t5",
                new Func<string, bool>((string s) =>
                {
                    if (builder.RuleSet.DefaultSet.HasAspect("PreventHidingAspect"))
                    {
                        return false;
                    }

                    var price = builder.RuleSet.DefaultSet.LowestPrice * builder.RuleSet.DefaultSet.ValueMultiplier;
                    return price < FilterPolishConfig.DiviT5BreakPoint;
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
