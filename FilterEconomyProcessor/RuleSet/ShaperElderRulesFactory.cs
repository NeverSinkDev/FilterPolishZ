using FilterEconomy.Processor;
using FilterPolishUtil;
using System;

namespace FilterEconomyProcessor.RuleSet
{
    public class ShaperElderRulesFactory
    {
        public static FilterEconomyRuleSet Generate(ConcreteEconomyRules ruleHost, string segment)
        {
            float valueMultiplierEffectiveness = 0.4f;

            var builder = new RuleSetBuilder(ruleHost)
                .SetSection(segment)
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
                    return !ruleHost.EconomyInformation.EconomyTierlistOverview[segment].ContainsKey(s);
                }));

            builder.AddRule("t1-82", "t1-1",
                new Func<string, bool>((string s) =>
                {
                    if (builder.RuleSet.DefaultSet.ValueMultiplier < 0.8f)
                    {
                        return false;
                    }

                    var price = GetPrice(82) * (1 + ((builder.RuleSet.DefaultSet.ValueMultiplier - 1) * valueMultiplierEffectiveness));
                    return price > FilterPolishConfig.T1BaseTypeBreakPoint;
                }), nextgroup: "t2");


            builder.AddRule("t1-84", "t1-2",
                new Func<string, bool>((string s) =>
                {
                    if (builder.RuleSet.DefaultSet.ValueMultiplier < 0.7f)
                    {
                        return false;
                    }

                    var price = GetPrice(84) * (1 + ((builder.RuleSet.DefaultSet.ValueMultiplier - 1) * valueMultiplierEffectiveness));
                    return price > FilterPolishConfig.T1BaseTypeBreakPoint;
                }), nextgroup: "t2");

            builder.AddRule("t1-86", "t1-3",
                new Func<string, bool>((string s) =>
                {
                    if (builder.RuleSet.DefaultSet.ValueMultiplier < 0.6f)
                    {
                        return false;
                    }

                    var price = GetPrice(86) * (1 + ((builder.RuleSet.DefaultSet.ValueMultiplier - 1) * valueMultiplierEffectiveness));
                    return price > FilterPolishConfig.T1BaseTypeBreakPoint;
                }), nextgroup: "t2");

            builder.AddRule("t2-80", "t2-1",
                new Func<string, bool>((string s) =>
                {
                    var price = GetPrice(82) * (1 + ((builder.RuleSet.DefaultSet.ValueMultiplier - 1) * valueMultiplierEffectiveness));
                    return price > FilterPolishConfig.T2BaseTypeBreakPoint;
                }), group: "t2");


            builder.AddRule("t2-85", "t2-2",
                new Func<string, bool>((string s) =>
                {
                    var price = Math.Max(GetPrice(86),GetPrice(85)) * (1 + ((builder.RuleSet.DefaultSet.ValueMultiplier - 1) * valueMultiplierEffectiveness));
                    return price > FilterPolishConfig.T2BaseTypeBreakPoint;
                }), group: "t2");

            builder.AddRule("rest", "rest",
                new Func<string, bool>((string s) =>
                {
                    return true;
                }));

            return builder.Build();

            float GetPrice(int level)
            {
                if (level > 86)
                {
                    return 0;
                }

                if (builder.RuleSet.DefaultSet.ftPrice.ContainsKey(level))
                {
                    return builder.RuleSet.DefaultSet.ftPrice[level];
                }
                else
                {
                    return GetPrice(level + 1);
                }
            }
        }
    }
}
