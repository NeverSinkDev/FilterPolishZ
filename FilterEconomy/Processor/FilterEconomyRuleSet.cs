using FilterCore.Constants;
using FilterEconomy.Model;
using FilterEconomy.Model.ItemInformationData;
using FilterPolishUtil.Collections;
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
        public Func<string, ItemList<NinjaItem>> DefaultItemQuery { get; set; }
        public ItemList<NinjaItem> DefaultSet { get; set; }
        public List<Action<TieringCommand>> PostProcessing { get; set; } = new List<Action<TieringCommand>>();
        public bool Enabled { get; set; } = true;
        public IEconomyProcessorData RuleHost { get; set; }
        public List<TieringCommand> SuggestionTarget {get; set;}

        public void GenerateAndAddSuggestions()
        {
            if (!Enabled)
            {
                return;
            }

            this.SuggestionTarget.Clear();
            this.SuggestionTarget.AddRange(this.GenerateSuggestions());
        }

        public IEnumerable<TieringCommand> GenerateSuggestions()
        {
            if (!Enabled)
            {
                yield break;
            }

            var result = RuleHost.EconomyInformation.EconomyTierlistOverview[this.GoverningSection]
                .Select(z => z.Key)
                .Select(x => this.ProcessItem(this.GoverningSection, x, x, RuleHost));

            foreach (var item in result)
            {
                foreach (var command in PostProcessing)
                {
                    command(item);
                }
                yield return item;
            }
        }

        public TieringCommand ProcessItem(string group, string basetype, string selectorString, IEconomyProcessorData processorData)
        {
            this.DefaultSet = DefaultItemQuery(selectorString);
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

        public Func<string, bool> Rule { get; set; }

        public TieringCommand Execute(string group, string basetype, IEconomyProcessorData processorData)
        {
            if (this.Rule(basetype))
            {
                return new TieringCommand()
                {
                    NewTier = TargetTier,
                    BaseType = basetype,
                    AppliedRule = RuleName
                };
            }

            return null;
        }
    }

    [DebuggerDisplay("{NewTier} // {BaseType}")]
    public class TieringCommand
    {
        public string AppliedRule { get; set; }
        public string BaseType { get; set; }
        public string OldTier { get; set; }
        public string NewTier { get; set; }
        public string Group { get; set; }
        public bool IsChange
        {
            get
            {
                if (this.Performed)
                {
                    return false;
                }

                if (this.NewTier == "???")
                {
                    return false;
                }

                if (this.OldTier.ToLower() == this.NewTier.ToLower())
                {
                    return false;
                }

                return !FilterConstants.IgnoredSuggestionTiers.Contains(this.OldTier.ToLower()) ||
                    !FilterConstants.IgnoredSuggestionTiers.Contains(this.NewTier.ToLower());
            }
       }

        public bool Change { get; set; }
        public bool Unsure { get; set; }
        public bool Performed { get; set; }

        public void Execute()
        {

        }
    }
}
