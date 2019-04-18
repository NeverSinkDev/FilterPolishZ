using FilterEconomy.Model;
using FilterEconomy.Processor;
using FilterPolishUtil;
using FilterPolishUtil.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishZ.Economy.RuleSet
{
    public class RuleSetBuilder
    {
        public string Section { get; set; }
        public FilterEconomyRuleSet RuleSet { get; set; } = new FilterEconomyRuleSet();
        public ConcreteEconomyRules RuleHost { get; set; }

        public RuleSetBuilder(ConcreteEconomyRules ruleHost)
        {
            this.RuleHost = ruleHost;
            RuleSet.RuleHost = ruleHost;
        }

        public RuleSetBuilder SetSection(string s)
        {
            this.Section = s;
            this.RuleSet.GoverningSection = s;
            return this;
        }

        public FilterEconomyRuleSet Build()
        {
            return this.RuleSet;
        }

        public RuleSetBuilder UseDefaultQuery()
        {
            this.RuleSet.DefaultItemQuery = new System.Func<string, FilterPolishUtil.Collections.ItemList<FilterEconomy.Model.NinjaItem>>((s) => this.RuleHost.EconomyInformation.EconomyTierlistOverview[this.Section][s]);
            return this;
        }

        public RuleSetBuilder UseCustomQuery(System.Func<string, FilterPolishUtil.Collections.ItemList<FilterEconomy.Model.NinjaItem>> customQuery)
        {
            this.RuleSet.DefaultItemQuery = customQuery;
            return this;
        }

        public RuleSetBuilder AddRule(string rulename, string targetTier, Func<string, bool> rule)
        {
            this.RuleSet.EconomyRules.Add(new FilterEconomyRule()
            {
                RuleName = rulename,
                TargetTier = targetTier,
                Rule = rule
            });

            return this;
        }

        public RuleSetBuilder AddDefaultIntegrationTarget()
        {
            this.RuleSet.SuggestionTarget = this.RuleHost.TierListFacade.Suggestions[this.Section];
            return this;
        }

        public RuleSetBuilder AddIntegrationTarget(List<TieringCommand> target)
        {
            this.RuleSet.SuggestionTarget = target;
            return this;
        }

        public RuleSetBuilder AddSimpleAspectContainerRule(string name, string tier, string aspect)
        {
            return this.AddRule(name, tier,
                new Func<string, bool>((string s) =>
                {
                    return this.RuleSet.DefaultSet.HasAspect(aspect);
                }));
        }

        public RuleSetBuilder AddSimpleComparisonRule(string name, string tier, float comparer)
        {
            return this.AddRule(name, tier,
                new Func<string, bool>((string s) =>
                {
                    var price = this.RuleSet.DefaultSet.LowestPrice;
                    return price > comparer;
                }));
        }

        public RuleSetBuilder AddRestRule()
        {
            return this.AddRule("rest", "rest",
                new Func<string, bool>((string s) =>
                {
                    return true;
                }));
        }

        public RuleSetBuilder AddPostProcessing(Action<TieringCommand> command)
        {
            this.RuleSet.PostProcessing.Add(command);
            return this;
        }

        public RuleSetBuilder AddDefaultPreprocessing()
        {
            this.RuleSet.PostProcessing.Add(new Action<TieringCommand>((TieringCommand tiercom) =>
            {
                tiercom.Group = this.Section;
                if (this.RuleHost.TierListFacade.ContainsTierInformationForBaseType(RuleSet.GoverningSection, tiercom.BaseType))
                {
                    tiercom.OldTier = RuleHost.TierListFacade.GetTiersForBasetype(RuleSet.GoverningSection, tiercom.BaseType).First().SubStringLast("->");
                    if (tiercom.OldTier == "ex")
                    {
                        tiercom.NewTier = "ex";
                        tiercom.AppliedRule = "exception";
                    }
                }
                else
                {
                    tiercom.OldTier = "rest";
                }
            }));

            return this;
        }
    }

    public static class RuleTools
    {
        public static bool HasAspect(this ItemList<NinjaItem> me, string s)
        {
            return me.Any(z => z.Aspects.Any(j => j.Name == s));
        }

        public static List<NinjaItem> OfAspect(this ItemList<NinjaItem> me, string s)
        {
            return me.Where(z => z.Aspects.Any(j => j.Name == s)).ToList();
        }

        public static List<NinjaItem> AspectCheck(this ItemList<NinjaItem> me, HashSet<string> include, HashSet<string> exclude)
        {
            return me.Where(
                z => z.Aspects.Any(x => include.Contains(x.Name) || include.Count == 0) &&
                     z.Aspects.All(x => !exclude.Contains(x.Name))).ToList();
        }

        public static bool AllItemsFullFill(this List<NinjaItem> me, HashSet<string> include, HashSet<string> exclude)
        {
            return me.All(
                z => z.Aspects.Any(x => include.Contains(x.Name) || include.Count == 0) &&
                     z.Aspects.All(x => !exclude.Contains(x.Name)));
        }
    }
}
