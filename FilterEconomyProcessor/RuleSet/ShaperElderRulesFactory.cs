using FilterEconomy.Processor;
using FilterPolishUtil;
using System;

namespace FilterEconomyProcessor.RuleSet
{
    public class ShaperElderRulesFactory
    {
        public static FilterEconomyRuleSet Generate(ConcreteEconomyRules ruleHost, string segment)
        {
            float valueMultiplierEffectiveness = 0.2f;

            var builder = new RuleSetBuilder(ruleHost)
                .SetSection(segment)
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .SkipInEarlyLeague()
                .AddDefaultIntegrationTarget();

            builder.AddRule("unknown", "unknown", s => !ruleHost.EconomyInformation.EconomyTierlistOverview[segment].ContainsKey(s));

            builder.AddRule("t1-82", "t1-1", s =>
                {
                    if (builder.Item.ValueMultiplier < 0.85f)
                    {
                        return false;
                    }

                    var price = GetPrice(82) * (1 + ((builder.Item.ValueMultiplier - 1) * valueMultiplierEffectiveness));
                    return price > FilterPolishConfig.BaseTypeT1BreakPoint;
                }, nextgroup: "t2");


            builder.AddRule("t1-84", "t1-2", s =>
                {
                    if (builder.Item.ValueMultiplier < 0.75f)
                    {
                        return false;
                    }

                    var price = GetPrice(84) * (1 + ((builder.Item.ValueMultiplier - 1) * valueMultiplierEffectiveness));
                    return price > FilterPolishConfig.BaseTypeT1BreakPoint;
                }, nextgroup: "t2");

            builder.AddRule("t1-86", "t1-3", s =>
                {
                    if (builder.Item.ValueMultiplier < 0.65f)
                    {
                        return false;
                    }

                    var price = GetPrice(86) * (1 + ((builder.Item.ValueMultiplier - 1) * valueMultiplierEffectiveness));
                    return price > FilterPolishConfig.BaseTypeT1BreakPoint;
                }, nextgroup: "t2");

            builder.AddRule("t2-80", "t2-1", s =>
                {
                    if (builder.Item.ValueMultiplier < 0.45f)
                    {
                        return false;
                    }

                    var price = GetPrice(82) * (1 + ((builder.Item.ValueMultiplier - 1) * valueMultiplierEffectiveness));
                    return price > FilterPolishConfig.BaseTypeT2BreakPoint * 1.2;
                }, group: "t2");


            builder.AddRule("t2-85", "t2-2", s =>
                {
                    if (builder.Item.ValueMultiplier < 0.1f)
                    {
                        return false;
                    }

                    var price = (GetPrice(85) + GetPrice(86) / 2) * (1 + ((builder.Item.ValueMultiplier - 1) * valueMultiplierEffectiveness));
                    return price > FilterPolishConfig.BaseTypeT2BreakPoint;
                }, group: "t2");

            builder.AddRule("rest", "rest", s => true);

            return builder.Build();

            float GetPrice(int level)
            {
                if (level > 86)
                {
                    return 0;
                }

                if (builder.Item.ftPrice?.ContainsKey(level) == true)
                {
                    return builder.Item.ftPrice[level];
                }
                else
                {
                    return GetPrice(level + 1);
                }
            }
        }
    }
}
