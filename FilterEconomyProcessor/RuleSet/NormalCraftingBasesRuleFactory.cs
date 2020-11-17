using FilterEconomy.Processor;
using FilterPolishUtil;
using System;

namespace FilterEconomyProcessor.RuleSet
{
    public class NormalCraftingBasesRuleFactory
    {
        public static FilterEconomyRuleSet Generate(ConcreteEconomyRules ruleHost, string segment)
        {
            var builder = new RuleSetBuilder(ruleHost)
                .SetSection(segment)
                .UseCustomQuery(((s) => ruleHost.EconomyInformation.EconomyTierlistOverview["generalcrafting"][s]))
                .AddDefaultPostProcessing()
                .SkipInEarlyLeague()
                .OverrideMinimalExaltedPriceThreshhold(60)
                .AddDefaultIntegrationTarget();

            builder.AddRule("t1-86-economy", "eco",
                new Func<string, bool>((string s) =>
                {
                    if (!FilterPolishConfig.SpecialBases.Contains(s))
                    {
                        return false;
                    }

                    var price = Math.Max(GetPrice(86), GetPrice(85));
                    return price > FilterPolishConfig.BaseTypeT1BreakPoint * 1.5f;
                }));

            builder.AddRule("rest", "rest",
                s => true);

            return builder.Build();

            float GetPrice(int level)
            {
                if (level > 86)
                {
                    return 0;
                }

                return builder.Item.ftPrice.ContainsKey(level) ? builder.Item.ftPrice[level] : GetPrice(level + 1);
            }
        }
    }
}
