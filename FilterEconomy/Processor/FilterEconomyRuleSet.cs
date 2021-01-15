﻿using FilterCore;
using FilterCore.Constants;
using FilterEconomy.Facades;
using FilterEconomy.Model;
using FilterEconomy.Model.ItemInformationData;
using FilterPolishUtil.Collections;
using FilterPolishUtil.Model;
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
        public float MinimalExaltedOrbPrice { get; set; } = FilterPolishUtil.FilterPolishConfig.TieringEnablingExaltedOrbPrice;

        public void GenerateAndAddSuggestions()
        {
            LoggingFacade.LogDebug($"Generating Suggestions for: {this.GoverningSection}");

            this.SuggestionTarget.Clear();
            this.SuggestionTarget.AddRange(this.GenerateSuggestions());

            if (!TierListFacade.GetInstance().EnabledSuggestions.ContainsKey(this.GoverningSection))
            {
                TierListFacade.GetInstance().EnabledSuggestions.Add(this.GoverningSection,Enabled);
            }
            else
            {
                TierListFacade.GetInstance().EnabledSuggestions[this.GoverningSection] = Enabled;
            }
        }

        public IEnumerable<TieringCommand> GenerateSuggestions()
        {
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

            if (!this.DefaultSet.Valid)
            {
                return new TieringCommand()
                {
                    AppliedRule = "disqualified",
                    BaseType = basetype,
                    NewTier = "rest",
                    Group = group,
                    Confidence = this.DefaultSet.ValueMultiplier
                };
            }

            string targetTier = "*";
            TieringCommand finalResult = null;
            TieringCommand currentResult = null;
            for (int i = 0; i < this.EconomyRules.Count; i++)
            {
                var currentRule = this.EconomyRules[i];
                currentResult = null;

                if (targetTier == currentRule.RuleGroup || targetTier == "ANY" || targetTier == "*")
                {
                    currentResult = currentRule.Execute(group, basetype, processorData);

                    if (currentResult != null)
                    {
                        if (finalResult == null)
                        {
                            finalResult = currentResult;
                        }
                        else
                        {
                            finalResult.NewTier = $"{finalResult.NewTier},{currentResult.NewTier}";
                            finalResult.AppliedRule = $"{finalResult.AppliedRule},{currentResult.AppliedRule}";
                        }

                        if (currentRule.NextRuleGroupToken == null)
                        {
                            finalResult.Confidence = this.DefaultSet.ValueMultiplier;
                            return finalResult;
                        }
                        else
                        {
                            targetTier = currentRule.NextRuleGroupToken;
                        }
                    }

                }
            }

            finalResult.Confidence = this.DefaultSet.ValueMultiplier;
            return finalResult;
        }
    }

    public class FilterEconomyRule
    {
        public string RuleName { get; set; }
        public string TargetTier { get; set; }
        public float Confidence { get; set; }
        public string RuleGroup { get; set; }
        public string NextRuleGroupToken { get; set; }

        public Func<string, bool> Rule { get; set; }

        public TieringCommand Execute(string group, string basetype, IEconomyProcessorData processorData)
        {
            if (this.Rule(basetype))
            {
                return new TieringCommand()
                {
                    NewTier = TargetTier,
                    BaseType = basetype,
                    AppliedRule = RuleName,
                    Confidence = this.Confidence
                };
            }

            return null;
        }
    }

    [DebuggerDisplay("{NewTier} // {BaseType}")]
    public class TieringCommand
    {
        public float Confidence { get; set; }
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

                return !FilterGenerationConfig.IgnoredSuggestionTiers.Contains(this.OldTier.ToLower()) ||
                    !FilterGenerationConfig.IgnoredSuggestionTiers.Contains(this.NewTier.ToLower());
            }
       }

        public bool LocalIgnore { get; set; } = false;

        public bool Performed { get; set; }

    }
}
