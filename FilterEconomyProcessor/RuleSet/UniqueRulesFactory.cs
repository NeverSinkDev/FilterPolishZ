using FilterEconomy.Processor;
using FilterPolishUtil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FilterEconomyProcessor.RuleSet
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
                    return builder.RuleSet.DefaultSet.LowestPrice > FilterPolishConfig.UniqueT1BreakPoint;
                }));

            builder.AddRule("t2", "t2",
                new Func<string, bool>((string s) =>
                {
                    return builder.RuleSet.DefaultSet.LowestPrice > FilterPolishConfig.UniqueT2BreakPoint;
                }));

            builder.AddRule("uncommon", "multispecial",
                new Func<string, bool>((string s) =>
                {
                    var fit = false;
                    if (builder.RuleSet.DefaultSet.HighestPrice > FilterPolishConfig.UniqueT2BreakPoint)
                    {
                        if (builder.RuleSet.DefaultSet.HasAspect("UncommonAspect"))
                        {
                            var relevantList = builder.RuleSet.DefaultSet.AspectCheck(new HashSet<string>() { "UncommonAspect" }, new HashSet<string>() { "BossDropAspect", "NonDropAspect", "LeagueDropAspect", "NonEventDropAspect" });

                            if (relevantList.Count > 0)
                            {
                                return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConfig.UniqueT2BreakPoint * FilterPolishConfig.UncommonAspectMultiplier;
                            }
                        }
                    }

                    return fit;
                }));

            builder.AddRule("ExpensiveTwin", "multispecial",
                new Func<string, bool>((string s) =>
                {
                    var relevantList = builder.RuleSet.DefaultSet.AspectCheck(new HashSet<string>() { "HandledAspect" }, new HashSet<string>() { "UncommonAspect", "BossDropAspect", "NonDropAspect", "LeagueDropAspect", "ProphecyResultAspect", "NonEventDropAspect" });

                    if (relevantList.Count > 1)
                    {
                        if (relevantList.Max(x => x.CVal) > FilterPolishConfig.UniqueT2BreakPoint * FilterPolishConfig.CommonTwinAspectMultiplier)
                        {
                            return true;
                        }
                    }

                    return false;
                }));

            builder.AddRule("highVariety", "multispecial",
                new Func<string, bool>((string s) =>
                {
                    var fit = false;
                    if (builder.RuleSet.DefaultSet.HighestPrice > FilterPolishConfig.UniqueT2BreakPoint * FilterPolishConfig.HighVarietyMultiplier)
                    {
                        if (builder.RuleSet.DefaultSet.HasAspect("HighVarietyAspect"))
                        {
                            var relevantList = builder.RuleSet.DefaultSet.AspectCheck(new HashSet<string>(), new HashSet<string>() { "BossDropAspect", "NonDropAspect", "LeagueDropAspect", "NonEventDropAspect"});

                            if (relevantList.Count > 0)
                            {
                                return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConfig.UniqueT2BreakPoint * FilterPolishConfig.HighVarietyMultiplier;
                            }
                        }
                    }

                    return fit;
                }));

            builder.AddRule("leagueDropAspect", "multileague",
                new Func<string, bool>((string s) =>
                {
                    var fit = false;
                    if (builder.RuleSet.DefaultSet.HighestPrice > FilterPolishConfig.UniqueT2BreakPoint)
                    {
                        if (builder.RuleSet.DefaultSet.HasAspect("LeagueDropAspect"))
                        {
                            var relevantList = builder.RuleSet.DefaultSet.AspectCheck(new HashSet<string>() { "LeagueDropAspect" }, new HashSet<string>() { "BossDropAspect", "NonDropAspect", "NonEventDropAspect" });

                            if (relevantList.Count > 0)
                            {
                                return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConfig.UniqueT2BreakPoint * FilterPolishConfig.LeagueDropAspectMultiplier;
                            }
                        }
                    }

                    return fit;
                }));

            builder.AddRule("BossOnly", "multispecial",
                new Func<string, bool>((string s) =>
                {
                    var fit = false;
                    if (builder.RuleSet.DefaultSet.HighestPrice > FilterPolishConfig.UniqueT2BreakPoint)
                    {
                        var relevantList = builder.RuleSet.DefaultSet.AspectCheck(new HashSet<string> { }, new HashSet<string>() { "NonDropAspect" });

                        if (relevantList.Count > 0 && relevantList.AllItemsFullFill(new HashSet<string>() { "BossDropAspect" }, new HashSet<string>()))
                        {
                            return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConfig.UniqueT2BreakPoint * FilterPolishConfig.LeagueDropAspectMultiplier;
                        }
                    }

                    return fit;
                }));

            // uniques that have changed in the latest league
            //builder.AddRule("Changed?", "metainfluenced",
            //    new Func<string, bool>((string s) =>
            //    {
            //        return builder.RuleSet.DefaultSet.HasAspect("ChangedAspect");
            //    }));

            // usually used for new leagues
            //builder.AddRule("MetaSave", "t2",
            //    new Func<string, bool>((string s) =>
            //    {
            //        return builder.RuleSet.DefaultSet.HasAspect("MetaBiasAspect");
            //    }));

            builder.AddRule("EarlyLeagueInterest", "earlyleague",
                new Func<string, bool>((string s) =>
                {
                    return builder.RuleSet.DefaultSet.HasAspect("EarlyLeagueInterestAspect");
                }));

            // extremely high value multibases that usually drop from boss encounters, but can also drop from special league events
            builder.AddRule("SuperLeagueUnique", "multileague",
                new Func<string, bool>((string s) =>
                {
                    if (builder.RuleSet.DefaultSet.HighestPrice > FilterPolishConfig.UniqueT1BreakPoint)
                    {
                        var relevantList = builder.RuleSet.DefaultSet.AspectCheck(new HashSet<string>() { "BossDropAspect", "LeagueDropAspect" }, new HashSet<string>() { "NonDropAspect", "NonEventDropAspect" });

                        if (relevantList.Count > 0)
                        {
                            return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConfig.SuperTierBreakPoint;
                        }
                    }
                    return false;
                }));

            builder.AddRule("prophecy", "prophecy",
                new Func<string, bool>((string s) =>
                {
                    var aspects = builder.RuleHost.ItemInformation["uniques", s];
                    if (aspects == null)
                    {
                        return false;
                    }

                    return builder.RuleSet.DefaultSet.HasAspect("ProphecyMaterialAspect");
                }));

            builder.AddRule("hideable", "hideable",
                new Func<string, bool>((string s) =>
                {
                    var maxprice = builder.RuleSet.DefaultSet.Max(x => x.CVal);
                    if (maxprice > FilterPolishConfig.UniqueT2BreakPoint * 0.35f)
                    {
                        return false;
                    }

                    if (builder.RuleSet.DefaultSet.AllItemsFullFill(new HashSet<string>() { }, new HashSet<string>(){ "HighVarietyAspect", "LeagueDropAspect", "NonEventDropAspect", "BossDropAspect", "EarlyLeagueInterestAspect", "IgnoreAspect", "UncommonAspect", "MetaBiasAspect", "AnchorAspect" }))
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
