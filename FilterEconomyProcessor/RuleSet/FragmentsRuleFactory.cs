using FilterEconomy.Processor;
using FilterPolishUtil;

namespace FilterEconomyProcessor.RuleSet
{
    public class FragmentsRuleFactory
    {
        public static FilterEconomyRuleSet Generate(ConcreteEconomyRules ruleHost)
        {
            var builder = new RuleSetBuilder(ruleHost)
                .SetSection("fragments")
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .SkipInEarlyLeague()
                .AddDefaultIntegrationTarget();

            builder.AddRule("No Tiering","???", s =>
                {
                    var isTierable = builder.Item.HasAspect("TierableFragmentAspect");
                    return !isTierable;
                });

            builder.AddRule("t1", "t1", s =>
                {
                    var isPredictable = builder.Item.HasAspect("PredictableDropAspect");

                    if (isPredictable)
                    {
                        return false;
                    }

                    var price = builder.Item.LowestPrice;
                    return price > FilterPolishConfig.MiscT1BreakPoint * 1.25f;
                });

            builder.AddRule("t1 predictable", "t1p", s =>
                {
                    var isPredictable = builder.Item.HasAspect("PredictableDropAspect");
                    var price = builder.Item.LowestPrice;

                    return isPredictable && price > FilterPolishConfig.MiscT1BreakPoint * 1.25f;
                });

            builder.AddRule("t2", "t2", s =>
                {
                    var price = builder.Item.LowestPrice;
                    return price > FilterPolishConfig.MiscT2BreakPoint * 1.25f;
                });

            builder.AddRule("t3", "t3", s =>
                {
                    var price = builder.Item.LowestPrice;
                    return price > FilterPolishConfig.MiscT3BreakPoint;
                });

            builder.AddEarlyLeagueHandling("t3");

            builder.AddRule("HidingPrevented", "t3", s => builder.Item.HasAspect("PreventHidingAspect"));

            builder.AddExplicitRest("t4", "t4");

            return builder.Build();
        }
    }
}
