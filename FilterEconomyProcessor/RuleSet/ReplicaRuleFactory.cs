using FilterEconomy.Processor;
using FilterPolishUtil;
using System.Linq;

namespace FilterEconomyProcessor.RuleSet
{
    public static class ReplicaRuleFactory
    {
        public static FilterEconomyRuleSet Generate(ConcreteEconomyRules ruleHost)
        {
            var builder = new RuleSetBuilder(ruleHost)
                .SetSection("unique->replicas")
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget();

            builder.AddSimpleComparisonRule("t1", "t1", FilterPolishConfig.UniqueT1BreakPoint);

            builder.AddSimpleComparisonRule("t2", "t2", FilterPolishConfig.UniqueT2BreakPoint);

            builder.AddRule("multi", "multi", x => builder.Item.Any(y => y.CVal > FilterPolishConfig.UniqueT2BreakPoint * FilterPolishConfig.CommonTwinAspectMultiplier));

            builder.AddExplicitRest("t3", "t3");

            return builder.Build();
        }
    }
}
