using FilterEconomy.Processor;
using FilterPolishUtil;
using System;

namespace FilterEconomyProcessor.RuleSet
{
    public class NormalCraftingBasesRuleFactory
    {
        public static FilterEconomyRuleSet Generate(ConcreteEconomyRules ruleHost, string segment)
        {
            float valueMultiplierEffectiveness = 0.4f;

            var builder = new RuleSetBuilder(ruleHost)
                .SetSection(segment)
                .UseCustomQuery(new System.Func<string, FilterPolishUtil.Collections.ItemList<FilterEconomy.Model.NinjaItem>>((s) => ruleHost.EconomyInformation.EconomyTierlistOverview["generalcrafting"][s]))
                .AddDefaultPostProcessing()
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
