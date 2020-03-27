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
            //var set = builder.Rulebuilder.Item.DefaultSet;

            builder.AddRule("unknown", "unknown",
                new Func<string, bool>((string s) =>
                {
                    return !ruleHost.EconomyInformation.EconomyTierlistOverview["uniques"].ContainsKey(s);
                }));

            builder.AddRule("t1", "t1",
                new Func<string, bool>((string s) =>
                {
                    return builder.Item.LowestPrice > FilterPolishConfig.UniqueT1BreakPoint;
                }));

            builder.AddRule("t2", "t2",
                new Func<string, bool>((string s) =>
                {
                    return builder.Item.LowestPrice > FilterPolishConfig.UniqueT2BreakPoint;
                }));

            builder.AddRule("uncommon", "multispecial",
                new Func<string, bool>((string s) =>
                {
                    var fit = false;
                    if (builder.Item.HighestPrice > FilterPolishConfig.UniqueT2BreakPoint)
                    {
                        if (builder.Item.HasAspect("UncommonAspect"))
                        {
                            var relevantList = builder.Item.AspectCheck(new HashSet<string>() { "UncommonAspect" }, new HashSet<string>() { "BossDropAspect", "NonDropAspect", "LeagueDropAspect", "NonEventDropAspect" });

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
                    var relevantList = builder.Item.AspectCheck(new HashSet<string>() { "HandledAspect" }, new HashSet<string>() { "UncommonAspect", "BossDropAspect", "NonDropAspect", "LeagueDropAspect", "ProphecyResultAspect", "NonEventDropAspect" });

                    if (relevantList.Count > 1)
                    {
                        if (relevantList.Max(x => x.CVal) > FilterPolishConfig.UniqueT2BreakPoint * FilterPolishConfig.CommonTwinAspectMultiplier)
                        {
                            return true;
                        }
                    }

                    return false;
                }));

            builder.AddRule("Expensive-Single-NonLeagueTwin", "multispecial",
                new Func<string, bool>((string s) =>
                {
                    var relevantList = builder.Item.AspectCheck(new HashSet<string>() { "HandledAspect" }, new HashSet<string>() { "UncommonAspect", "BossDropAspect", "NonDropAspect", "LeagueDropAspect", "ProphecyResultAspect", "NonEventDropAspect" });

                    if (relevantList.Count == 1)
                    {
                        if (relevantList.Max(x => x.CVal) > FilterPolishConfig.UniqueT2BreakPoint)
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
                    if (builder.Item.HighestPrice > FilterPolishConfig.UniqueT2BreakPoint * FilterPolishConfig.HighVarietyMultiplier)
                    {
                        if (builder.Item.HasAspect("HighVarietyAspect"))
                        {
                            var relevantList = builder.Item.AspectCheck(new HashSet<string>(), new HashSet<string>() { "BossDropAspect", "NonDropAspect", "LeagueDropAspect", "NonEventDropAspect"});

                            if (relevantList.Count > 0)
                            {
                                return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConfig.UniqueT2BreakPoint * FilterPolishConfig.HighVarietyMultiplier;
                            }
                        }
                    }

                    return fit;
                }));

            builder.AddRule("leagueDropAspect", "multispecial",
                new Func<string, bool>((string s) =>
                {
                    var fit = false;
                    if (builder.Item.HighestPrice > FilterPolishConfig.UniqueT2BreakPoint)
                    {
                        if (builder.Item.HasAspect("LeagueDropAspect"))
                        {
                            var relevantList = builder.Item.AspectCheck(new HashSet<string>() { "LeagueDropAspect" }, new HashSet<string>() { "BossDropAspect", "NonDropAspect", "NonEventDropAspect" });

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
                    if (builder.Item.HighestPrice > FilterPolishConfig.UniqueT2BreakPoint)
                    {
                        var relevantList = builder.Item.AspectCheck(new HashSet<string> { }, new HashSet<string>() { "NonDropAspect" });

                        if (relevantList.Count > 0 && relevantList.AllItemsFullFill(new HashSet<string>() { "BossDropAspect" }, new HashSet<string>()))
                        {
                            return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConfig.UniqueT2BreakPoint * FilterPolishConfig.LeagueDropAspectMultiplier;
                        }
                    }

                    return fit;
                }));

            builder.AddRule("EarlyLeagueInterest", "earlyleague",
                new Func<string, bool>((string s) =>
                {
                    return builder.Item.HasAspect("EarlyLeagueInterestAspect");
                }));

            // extremely high value multibases that usually drop from boss encounters, but can also drop from special league events
            builder.AddRule("SuperLeagueUnique", "multispecial",
                new Func<string, bool>((string s) =>
                {
                    if (builder.Item.HighestPrice > FilterPolishConfig.UniqueT1BreakPoint)
                    {
                        var relevantList = builder.Item.AspectCheck(new HashSet<string>() { "BossDropAspect", "LeagueDropAspect" }, new HashSet<string>() { "NonDropAspect", "NonEventDropAspect" });

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

                    return builder.Item.HasAspect("ProphecyMaterialAspect");
                }));

            builder.AddRule("ExpensiveOrBoss", "t3boss",
                new Func<string, bool>((string s) =>
                {
                    var relevantList = builder.Item.AspectCheck(new HashSet<string> { }, new HashSet<string>() { "NonDropAspect" });
                    var bossDrop = builder.Item.HasAspect("BossDropAspect");

                    if (relevantList.Count == 0)
                    {
                        return false;
                    }

                    return bossDrop || builder.Item.LowestPrice < FilterPolishConfig.UniqueT2Base && builder.Item.HighestPrice > FilterPolishConfig.UniqueT2Base;
                }));

            builder.AddRule("hideable", "hideable",
                new Func<string, bool>((string s) =>
                {
                    var maxprice = builder.Item.Max(x => x.CVal);
                    if (maxprice > FilterPolishConfig.UniqueT2BreakPoint * 0.35f)
                    {
                        return false;
                    }

                    if (builder.Item.AllItemsFullFill(new HashSet<string>() { }, new HashSet<string>(){ "HighVarietyAspect", "LeagueDropAspect", "NonEventDropAspect", "BossDropAspect", "IgnoreAspect", "UncommonAspect", "MetaBiasAspect", "AnchorAspect" }))
                    {
                        return true;
                    }

                    return false;
                }));

            builder.AddExplicitRest("t3", "t3");

            return builder.Build();
        }
    }
}
