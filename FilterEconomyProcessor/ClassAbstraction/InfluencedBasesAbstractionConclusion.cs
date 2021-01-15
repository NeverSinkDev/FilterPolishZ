using FilterPolishUtil.Model;
using System.Linq;

namespace FilterEconomyProcessor.ClassAbstraction
{
    public class InfluencedBasesAbstractionConclusions
    {
        public InfluencedBasesAbstractionConclusions()
        {

        }

        public void Execute()
        {
            // base type abstraction facade
            var bta = InfluencedBasesAbstractionOverview.InfluencedItemInformation;

            foreach (var comparison in ConcreteAbstractionComparisons.ComparisonObjects)
            {
                foreach (var influence in bta.InfluenceTypes)
                {
                    var classes = influence.Value.GetBreakPointClasses(comparison);
                    LoggingFacade.LogDebug
                        ($"{influence}:{comparison.RuleName}::" +
                        $"{string.Join(" ", classes.Select(x => x.ClassName))}");
                }
            }
        }
    }
}
