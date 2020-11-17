using System.Collections.Generic;
using FilterEconomy.Processor;
using FilterPolishUtil;

namespace FilterEconomyProcessor.RuleSet
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

            builder.AddRule("unknown", "unknown", s => !ruleHost.EconomyInformation.EconomyTierlistOverview["divination"].ContainsKey(s));

            builder.AddRule("t1", "t1", s =>
                {
                    var price = builder.Item.LowestPrice * builder.Item.ValueMultiplier;
                    return price > FilterPolishConfig.DiviT1BreakPoint;
                });

            builder.AddRule("t2", "t2", s =>
                {
                    var price = builder.Item.LowestPrice * builder.Item.ValueMultiplier;
                    return price > FilterPolishConfig.DiviT2BreakPoint;
                });

            builder.AddRule("SingleCardSave", "t2", s => builder.Item.HasAspect("SingleCardAspect"));

            builder.AddSimpleAspectContainerRule("EARLYBuffAspect", "t2", "BuffAspect");

            builder.AddRule("t3", "t3", s =>
                {
                    // We reduce the tiering of CurrencyType items here. They should be more weighted towards t4c, since poe.ninja seems to price them higher than they are.
                    var price = builder.Item.LowestPrice * builder.Item.ValueMultiplier * (builder.Item.HasAspect("CurrencyTypeAspect") ? 0.8f : 1f);

                    return price > FilterPolishConfig.DiviT3BreakPoint;
                });

            builder.AddRule("TimelessSave", "t3", s => builder.Item.HasAspect("TimelessResultAspect"));

            builder.AddEarlyLeagueHandling("t3");
            
            builder.AddEarlyLeagueProtectionBlock("t2", new HashSet<string> { "t1", "t2" }, "earlyProtHIGH");

            builder.AddRule("CurrencySaveT4", "t4c", s =>
                {
                    var price = builder.Item.LowestPrice * builder.Item.ValueMultiplier;

                    if (builder.Item.HasAspect("CurrencyTypeAspect"))
                    {
                        if (builder.Item.HasAspect("PoorDropAspect"))
                        {
                            return (price > FilterPolishConfig.DiviT5BreakPoint * 3);
                        }

                        return (price > FilterPolishConfig.DiviT5BreakPoint * 1.5);
                    }

                    return false;
                });

            builder.AddRule("CurrencySaveT4X", "t4c", s =>
                {
                    var price = builder.Item.LowestPrice * builder.Item.ValueMultiplier;

                    if (builder.Item.HasAspect("CurrencyTypeAspect"))
                    {
                        return builder.Item.HasAspect("FarmableOrbAspect") || builder.Item.HasAspect("PreventHidingAspect");
                    }

                    return false;
                });

            builder.AddEarlyLeagueProtectionBlock("t4", new HashSet<string> { "t3" }, "earlyProtLOW");

            builder.AddRule("CurrencySaveT5", "t5c", s => builder.Item.HasAspect("CurrencyTypeAspect"));

            builder.AddSimpleAspectContainerRule("EARLYNerfAspect", "t2", "NerfAspect");

            builder.AddRule("RandomSave", "t4", s => builder.Item.HasAspect("LargeRandomPoolAspect"));


            builder.AddRule("PoorCard", "t5", s =>
                {
                    var price = builder.Item.LowestPrice * builder.Item.ValueMultiplier;
                    return (price < (FilterPolishConfig.DiviT5BreakPoint * 2.5f) && builder.Item.HasAspect("PoorDropAspect"));
                });

            builder.AddRule("T5RedemptionPrevented", "t5", s =>
                {
                    if (builder.GetTierOfItem(s).Contains("t5"))
                    {
                        var price = builder.Item.LowestPrice * builder.Item.ValueMultiplier;
                        return price < FilterPolishConfig.DiviT5BreakPoint * 2;
                    }

                    return false;
                });

            builder.AddRule("t5", "t5", s =>
                {
                    if (builder.Item.HasAspect("PreventHidingAspect"))
                    {
                        return false;
                    }

                    var price = builder.Item.LowestPrice * builder.Item.ValueMultiplier;
                    return price < FilterPolishConfig.DiviT5BreakPoint;
                });

            builder.AddExplicitRest("t4", "t4");

            return builder.Build();
        }

    }
}
