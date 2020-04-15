using FilterEconomy.Facades;
using FilterEconomy.Model;
using FilterEconomy.Processor;
using FilterPolishUtil;
using FilterPolishUtil.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterEconomyProcessor.RuleSet
{
    public class RuleSetBuilder
    {
        public string Section { get; set; }
        public FilterEconomyRuleSet RuleSet { get; set; } = new FilterEconomyRuleSet();
        public ItemList<NinjaItem> Item => this.RuleSet.DefaultSet;
        public ConcreteEconomyRules RuleHost { get; set; }

        public RuleSetBuilder(ConcreteEconomyRules ruleHost)
        {
            this.RuleHost = ruleHost;
            RuleSet.RuleHost = ruleHost;

            this.AddSafetyRule();

            this.AddRule("ANCHOR", "ANCHOR",
                new Func<string, bool>((string s) =>
                {
                    return this.Item.HasAspect("AnchorAspect");
                }));
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

        public RuleSetBuilder OverrideMinimalExaltedPriceThreshhold(float price)
        {
            this.RuleSet.MinimalExaltedOrbPrice = price;
            return this;
        }

        public RuleSetBuilder UseCustomQuery(System.Func<string, FilterPolishUtil.Collections.ItemList<FilterEconomy.Model.NinjaItem>> customQuery)
        {
            this.RuleSet.DefaultItemQuery = customQuery;
            return this;
        }

        public RuleSetBuilder AddRule(string rulename, string targetTier, Func<string, bool> rule, string group = default(string), string nextgroup = default(string))
        {
            this.RuleSet.EconomyRules.Add(new FilterEconomyRule()
            {
                RuleName = rulename,
                TargetTier = targetTier,
                Rule = rule,
                RuleGroup = group,
                NextRuleGroupToken = nextgroup
            });

            return this;
        }

        public RuleSetBuilder AddDefaultIntegrationTarget()
        {
            this.RuleSet.SuggestionTarget = this.RuleHost.TierListFacade.Suggestions[this.Section];
            return this;
        }

        public RuleSetBuilder SkipInEarlyLeague()
        {
            if (EconomyRequestFacade.GetInstance().IsEarlyLeague() == true)
            {
                this.RuleSet.Enabled = false;
            }

            return this;
        }

        public RuleSetBuilder LimitExecutionMode(ExecutionMode mode)
        {
            if (FilterPolishConfig.ApplicationExecutionMode != mode)
            {
                this.RuleSet.Enabled = false;
            }

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
                    return Item.HasAspect(aspect);
                }));
        }

        public RuleSetBuilder AddSimpleComparisonRule(string name, string tier, float comparer)
        {
            return this.AddRule(name, tier,
                new Func<string, bool>((string s) =>
                {
                    var price = Item.LowestPrice;
                    return price > comparer;
                }));
        }

        public RuleSetBuilder AddAverageComparison(string name, string tier, float comparer)
        {
            return this.AddRule(name, tier,
                new Func<string, bool>((string s) =>
                {
                    var price = (Item.LowestPrice + Item.HighestPrice) / 2;
                    return price > comparer;
                }));
        }

        public RuleSetBuilder AddSafetyRule()
        {
            return this.AddRule("No Data Found", "???",
                new Func<string, bool>((string s) =>
                {
                    var price = Item.LowestPrice;
                    if (price == 0)
                    {
                        return true;
                    }

                    if (this.RuleSet.GoverningSection.ToLower() == "fragments" || this.RuleSet.GoverningSection.ToLower() == "currency")
                    {
                        return false;
                    }

                    if (this.Item?.FirstOrDefault(x => x.Name == s)?.IndexedCount == 0)
                    {
                        return true;
                    }

                    return false;
                }));
        }

        public RuleSetBuilder AddSimpleReversedComparisonRule(string name, string tier, float comparer)
        {
            return this.AddRule(name, tier,
                new Func<string, bool>((string s) =>
                {
                    var price = Item.LowestPrice;
                    return price < comparer;
                }));
        }

        public RuleSetBuilder AddEarlyLeagueHandling(string tier)
        {
            return this.AddRule("EarlyLeagueInterest", tier,
                new Func<string, bool>((string s) =>
                {
                    return Item.HasAspect("EarlyLeagueInterestAspect");
                }));
        }

        public RuleSetBuilder AddPoorDropRoutine(string tier, float comparer, float weight = 2.0f, bool redemptionLag = true, float redemptionWeight = 1.5f)
        {
            // Low value drops tend to have a high noise amplitude on Poe.ninja/the actual economy.
            // Items marked as a "PoorItem", need to climb a certain threshold to leave the low item tier
            this.AddRule($"{tier} (PoorItem)", tier,
                new Func<string, bool>((string s) =>
                {
                    var price = this.Item.LowestPrice;
                    return (price < (comparer * weight) && this.Item.HasAspect("PoorDropAspect") && !this.Item.HasAspect("EarlyLeagueInterestAspect") && !this.Item.HasAspect("PreventHidingAspect"));
                }));

            this.AddRule($"{tier} (FailedRedemption)", tier,
                new Func<string, bool>((string s) =>
                {
                    if (this.Item.HasAspect("EarlyLeagueInterestAspect") || this.Item.HasAspect("PreventHidingAspect"))
                    {
                        return false;
                    }

                    if (this.GetTierOfItem(s).Contains(tier))
                    {
                        var price = this.Item.LowestPrice;
                        return price < comparer * redemptionWeight;
                    }

                    return false;
                }));

            this.AddRule(tier, tier,
                new Func<string, bool>((string s) =>
                {
                    if (this.Item.HasAspect("EarlyLeagueInterestAspect") || this.Item.HasAspect("PreventHidingAspect"))
                    {
                        return false;
                    }

                    var price = this.Item.LowestPrice;
                    return (price < comparer);
                }));

            return this;
        }

        public RuleSetBuilder AddRestRule()
        {
            return this.AddRule("rest", "rest",
                new Func<string, bool>((string s) =>
                {
                    return true;
                }));
        }

        public RuleSetBuilder AddExplicitRest(string rule, string tier)
        {
            return this.AddRule(rule, tier,
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

        public RuleSetBuilder AddDefaultPostProcessing()
        {
            this.RuleSet.PostProcessing.Add(new Action<TieringCommand>((TieringCommand tiercom) =>
            {
                tiercom.Group = this.Section;

                if (this.RuleHost.TierListFacade.ContainsTierInformationForBaseType(RuleSet.GoverningSection, tiercom.BaseType))
                {
                    tiercom.OldTier = GetTierOfItem(tiercom.BaseType);

                    if (tiercom.OldTier.Contains("ex"))
                    {
                        tiercom.NewTier = tiercom.OldTier;
                        tiercom.AppliedRule = "exception";
                    }
                }
                else
                {
                    tiercom.OldTier = "rest";
                }

                if (tiercom.AppliedRule.ToLower() == "anchor")
                {
                    tiercom.NewTier = tiercom.OldTier;
                }
            }));

            return this;
        }

        public string GetTierOfItem(string basetype)
        {
            if (!RuleHost.TierListFacade.ContainsTierInformationForBaseType(this.RuleSet.GoverningSection, basetype))
            {
                return "rest";
            }

            return string.Join(",", 
                RuleHost.TierListFacade.GetTiersForBasetype(this.RuleSet.GoverningSection, basetype)
                .Select(x => x.SubStringLast("->")));
        }
    }
}
