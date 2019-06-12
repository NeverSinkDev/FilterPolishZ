using FilterEconomy.Processor;
using FilterPolishUtil.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishZ.Economy.RuleSet
{
    public static class UniqueRulesFactory
    {
        public static FilterEconomyRuleSet Generate(ConcreteEconomyRules ruleHost)
        {
            var builder = new RuleSetBuilder(ruleHost)
                .SetSection("uniques")
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                .AddDefaultIntegrationTarget();
            //var set = builder.Rulebuilder.RuleSet.DefaultSet.DefaultSet;

            builder.AddRule("ANCHOR", "ANCHOR",
                new Func<string, bool>((string s) =>
                {
                    return builder.RuleSet.DefaultSet.HasAspect("AnchorAspect");
                }));

            builder.AddRule("unknown", "unknown",
                new Func<string, bool>((string s) =>
                {
                    return !ruleHost.EconomyInformation.EconomyTierlistOverview["uniques"].ContainsKey(s);
                }));

            builder.AddRule("t1", "t1",
                new Func<string, bool>((string s) =>
                {
                    return builder.RuleSet.DefaultSet.LowestPrice > FilterPolishConstants.T1BreakPoint;
                }));

            builder.AddRule("t2", "t2",
                new Func<string, bool>((string s) =>
                {
                    return builder.RuleSet.DefaultSet.LowestPrice > FilterPolishConstants.T2BreakPoint;
                }));

            builder.AddRule("uncommon", "multispecial",
                new Func<string, bool>((string s) =>
                {
                    var fit = false;
                    if (builder.RuleSet.DefaultSet.HighestPrice > FilterPolishConstants.T2BreakPoint)
                    {
                        if (builder.RuleSet.DefaultSet.HasAspect("UncommonAspect"))
                        {
                            var relevantList = builder.RuleSet.DefaultSet.AspectCheck(new HashSet<string>() { "UncommonAspect" }, new HashSet<string>() { "BossDropAspect", "NonDropAspect", "LeagueDropAspect" });

                            if (relevantList.Count > 0)
                            {
                                return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConstants.T2BreakPoint * FilterPolishConstants.UncommonAspectMultiplier;
                            }
                        }
                    }

                    return fit;
                }));

            builder.AddRule("highVariety", "multispecial",
                new Func<string, bool>((string s) =>
                {
                    var fit = false;
                    if (builder.RuleSet.DefaultSet.HighestPrice > FilterPolishConstants.T2BreakPoint * FilterPolishConstants.HighVarietyMultiplier)
                    {
                        if (builder.RuleSet.DefaultSet.HasAspect("HighVarietyAspect"))
                        {
                            var relevantList = builder.RuleSet.DefaultSet.AspectCheck(new HashSet<string>() { "HighVarietyAspect" }, new HashSet<string>() { "BossDropAspect", "NonDropAspect", "LeagueDropAspect" });

                            if (relevantList.Count > 0)
                            {
                                return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConstants.T2BreakPoint * FilterPolishConstants.HighVarietyMultiplier;
                            }
                        }
                    }

                    return fit;
                }));

            builder.AddRule("leagueDropAspect", "multileague",
                new Func<string, bool>((string s) =>
                {
                    var fit = false;
                    if (builder.RuleSet.DefaultSet.HighestPrice > FilterPolishConstants.T2BreakPoint)
                    {
                        if (builder.RuleSet.DefaultSet.HasAspect("LeagueDropAspect"))
                        {
                            var relevantList = builder.RuleSet.DefaultSet.AspectCheck(new HashSet<string>() { "LeagueDropAspect" }, new HashSet<string>() { "BossDropAspect", "NonDropAspect" });

                            if (relevantList.Count > 0)
                            {
                                return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConstants.T2BreakPoint * FilterPolishConstants.LeagueDropAspectMultiplier;
                            }
                        }
                    }

                    return fit;
                }));

            builder.AddRule("BossOnly", "multispecial",
                new Func<string, bool>((string s) =>
                {
                    var fit = false;
                    if (builder.RuleSet.DefaultSet.HighestPrice > FilterPolishConstants.T2BreakPoint)
                    {
                        var relevantList = builder.RuleSet.DefaultSet.AspectCheck(new HashSet<string> { }, new HashSet<string>() { "NonDropAspect" });

                        if (relevantList.Count > 0 && relevantList.AllItemsFullFill(new HashSet<string>() { "BossDropAspect" }, new HashSet<string>()))
                        {
                            return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConstants.T2BreakPoint * FilterPolishConstants.LeagueDropAspectMultiplier;
                        }
                    }

                    return fit;
                }));

            builder.AddRule("Changed?", "metainfluenced",
                new Func<string, bool>((string s) =>
                {
                    return builder.RuleSet.DefaultSet.HasAspect("ChangedAspect");
                }));

            builder.AddRule("MetaSave", "t2",
                new Func<string, bool>((string s) =>
                {
                    return builder.RuleSet.DefaultSet.HasAspect("MetaBiasAspect");
                }));

            builder.AddRule("???", "???",
                new Func<string, bool>((string s) =>
                {
                    var fit = false;
                    if (builder.RuleSet.DefaultSet.HighestPrice > FilterPolishConstants.T2BreakPoint)
                    {
                        if (builder.RuleSet.DefaultSet.HasAspect("LeagueDropAspect"))
                        {
                            return builder.RuleSet.DefaultSet.OfAspect("LeagueDropAspect").OrderByDescending(x => x.CVal).First().CVal > FilterPolishConstants.T2BreakPoint;
                        }
                    }

                    return fit;
                }));

            builder.AddRule("prophecy", "prophecy",
                new Func<string, bool>((string s) =>
                {
                    var aspects = builder.RuleHost.ItemInformation["uniques", s];
                    if (aspects == null)
                    {
                        return true;
                    }

                    return builder.RuleSet.DefaultSet.HasAspect("ProphecyMaterialAspect");
                }));

            builder.AddRule("hideable", "hideable",
                new Func<string, bool>((string s) =>
                {
                    var maxprice = builder.RuleSet.DefaultSet.Max(x => x.CVal);
                    if (maxprice > FilterPolishConstants.T2BreakPoint * 0.8f)
                    {
                        return false;
                    }

                    if (builder.RuleSet.DefaultSet.AllItemsFullFill(new HashSet<string>() { }, new HashSet<string>(){ "HighVarietyAspect", "LeagueDropAspect", "BossDropAspect", "EarlyLeagueInterestAspect", "IgnoreAspect", "UncommonAspect", "MetaBiasAspect", "AnchorAspect" }))
                    {
                        return true;
                    }

                    return false;
                }));

            builder.AddRule("rest", "rest",
                new Func<string, bool>((string s) =>
                {
                    return true;
                }));

            return builder.Build();
        }
    }
}
