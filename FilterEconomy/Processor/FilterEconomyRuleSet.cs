using FilterEconomy.Model.ItemInformationData;
using FilterUtilModels.Economy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterEconomy.Processor
{
    public class FilterEconomyRuleSet
    {
        public string GoverningSection { get; set; }
        public List<FilterEconomyRule> EconomyRules { get; set; } = new List<FilterEconomyRule>();

        public TieringCommand ProcessItem(string group, string basetype, IEconomyProcessorData processorData)
        {
            return this.EconomyRules.Select(
                x => 
                    x.Execute(group, basetype, processorData))
                    .FirstOrDefault(z => z != null);
        }
    }

    public class FilterEconomyRule
    {
        public string RuleName { get; set; }
        public string TargetTier { get; set; }
        public Func<string,bool> Rule { get; set; }

        public TieringCommand Execute(string group, string basetype, IEconomyProcessorData processorData)
        {
            if (this.Rule(basetype))
            {
                return new TieringCommand()
                {
                    NewTier = TargetTier,
                    BaseType = basetype
                };
            }

            return null;
        }
    }

    [DebuggerDisplay("{NewTier} // {BaseType}")]
    public class TieringCommand
    {
        public float Price { get; set; }
        public string BaseType { get; set; }
        public string UniqueName { get; set; }
        public string OldTier { get; set; }
        public string NewTier { get; set; }
        public string Group { get; set; }

        public void Execute()
        {

        }
    }
}
