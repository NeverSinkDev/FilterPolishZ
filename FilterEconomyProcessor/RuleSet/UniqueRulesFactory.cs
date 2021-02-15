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

            builder.AddRule("unknown", "unknown", s => !ruleHost.EconomyInformation.EconomyTierlistOverview["uniques"].ContainsKey(s));

            builder.AddRule("unhandled", "???", s => builder.Item.AllUnhandled());

            builder.AddRule("t1", "t1", s => builder.Item.LowestPrice > FilterPolishConfig.UniqueT1BreakPoint);

            builder.AddRule("t2", "t2", s => builder.Item.LowestPrice > FilterPolishConfig.UniqueT2BreakPoint);

            // builder.AddSimpleAspectContainerRule("EARLYBuffAspect", "t2", "BuffAspect");

            // items with one expensive rare version and one common, less rare verison are handled by this rule.
            builder.AddRule("uncommon", "multispecial", s =>
                {
                    if (!(builder.Item.HighestPrice > FilterPolishConfig.UniqueT2BreakPoint)) return false;
                    if (!builder.Item.HasAspect("UncommonAspect")) return false;
                    
                    var relevantList = builder.Item.AspectCheck(new HashSet<string>() { "UncommonAspect" }, new HashSet<string>() { "BossDropAspect", "NonDropAspect", "LeagueDropAspect", "NonEventDropAspect", "ReplicaAspect" });

                    if (relevantList.Count > 0)
                    {
                        return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConfig.UniqueT2BreakPoint * FilterPolishConfig.UncommonAspectMultiplier;
                    }

                    return false;
                });

            // items with expensive and non-expensive commonly droppable versions.
            builder.AddRule("ExpensiveTwin", "multispecial", s =>
                {
                    var relevantList = builder.Item.AspectCheck(new HashSet<string>() { "HandledAspect" }, new HashSet<string>() { "UncommonAspect", "BossDropAspect", "NonDropAspect", "LeagueDropAspect", "ProphecyResultAspect", "NonEventDropAspect" });
                    if (relevantList.Count <= 1) return false;
                    return relevantList.Max(x => x.CVal) > FilterPolishConfig.UniqueT2BreakPoint * FilterPolishConfig.CommonTwinAspectMultiplier;
                });

            // items with expensive and non-expensive commonly droppable versions.
            builder.AddRule("Expensive-Single-NonLeagueTwin", "multispecial", s =>
                {
                    var relevantList = builder.Item.AspectCheck(new HashSet<string>() { "HandledAspect" }, new HashSet<string>() { "UncommonAspect", "BossDropAspect", "NonDropAspect", "LeagueDropAspect", "ProphecyResultAspect", "NonEventDropAspect" });
                    if (relevantList.Count != 1) return false;
                    return relevantList.Max(x => x.CVal) > FilterPolishConfig.UniqueT2BreakPoint;
                });

            builder.AddRule("highVariety", "multispecial", s =>
                {
                    if (builder.Item.HighestPrice > FilterPolishConfig.UniqueT2BreakPoint * FilterPolishConfig.HighVarietyMultiplier)
                    {
                        if (!builder.Item.HasAspect("HighVarietyAspect")) return false;
                        
                        var relevantList = builder.Item.AspectCheck(new HashSet<string>(), new HashSet<string>() { "BossDropAspect", "NonDropAspect", "LeagueDropAspect", "NonEventDropAspect" });
                        
                        if (relevantList.Count > 0)
                        {
                            return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConfig.UniqueT2BreakPoint * FilterPolishConfig.HighVarietyMultiplier;
                        }
                    }

                    return false;
                });

            builder.AddRule("leagueDropAspect", "multispecial", s =>
            {
                if (!(builder.Item.HighestPrice > FilterPolishConfig.UniqueT2BreakPoint)) return false;
                    if (!builder.Item.HasAspect("LeagueDropAspect")) return false;
                    var relevantList = builder.Item.AspectCheck(new HashSet<string>() { "LeagueDropAspect" }, new HashSet<string>() { "BossDropAspect", "NonDropAspect", "NonEventDropAspect" });

                    if (relevantList.Count > 0)
                    {
                        return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConfig.UniqueT2BreakPoint * FilterPolishConfig.LeagueDropAspectMultiplier;
                    }

                    return false;
                });

            builder.AddRule("BossOnly", "multispecial", s =>
                {
                    if (!(builder.Item.HighestPrice > FilterPolishConfig.UniqueT2BreakPoint)) return false;
                    var relevantList = builder.Item.AspectCheck(new HashSet<string> { }, new HashSet<string>() { "NonDropAspect" });

                    if (relevantList.Count > 0 && relevantList.AllItemsFullFill(new HashSet<string>() { "BossDropAspect" }, new HashSet<string>()))
                    {
                        return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConfig.UniqueT2BreakPoint * FilterPolishConfig.LeagueDropAspectMultiplier;
                    }

                    return false;
                });

            builder.AddEarlyLeagueHandling("earlyleague");

            // extremely high value multibases that usually drop from boss encounters, but can also drop from special league events
            builder.AddRule("SuperLeagueUnique", "multispecial", s =>
                {
                    if (!(builder.Item.HighestPrice > FilterPolishConfig.UniqueT1BreakPoint)) return false;
                    var relevantList = builder.Item.AspectCheck(new HashSet<string>() { "LeagueDropAspect" }, new HashSet<string>() { "NonDropAspect", "NonEventDropAspect" });
                    if (relevantList.Count > 0)
                    {
                        return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConfig.SuperTierBreakPoint;
                    }
                    return false;
                });

            builder.AddRule("ExpensiveOrBoss", "t3boss", s =>
                {
                    var bossDrop = builder.Item.HasAspect("BossDropAspect");
                    return bossDrop || builder.Item.LowestPrice < FilterPolishConfig.UniqueT2BreakPoint && builder.Item.HighestPrice > FilterPolishConfig.UniqueT2BreakPoint;
                });

            //builder.AddRule("prophecy", "prophecy",
            //    new Func<string, bool>((string s) =>
            //    {
            //        var aspects = builder.RuleHost.ItemInformation["uniques", s];
            //        if (aspects == null)
            //        {
            //            return false;
            //        }

            //        return builder.Item.HasAspect("ProphecyMaterialAspect");
            //    }));

            builder.AddSimpleAspectContainerRule("EARLYNerfAspect", "t3", "NerfAspect");
            builder.AddEarlyLeagueProtectionBlock("t2", new HashSet<string>() { "t1" }, "earlyProtHIGH");
            builder.AddEarlyLeagueProtectionBlock("t3", new HashSet<string>() { "t2" }, "earlyProtLOW");

            builder.AddRule("hideable-nondrop", "hideable2", s =>
            {
                var aspectTest = builder.Item.AllItemsFullFill(new HashSet<string>() { }, new HashSet<string>() { "HighVarietyAspect", "NonEventDropAspect", "PreventHidingAspect" });

                if (!aspectTest)
                {
                    return false;
                }

                var relevantListNonDrop = builder.Item.AspectCheck(new HashSet<string>() { "NonDropAspect", "ProphecyResultAspect" }, new HashSet<string>() { });
                var relevantListRest = builder.Item.AspectCheck(new HashSet<string>() {  }, new HashSet<string>() { "NonDropAspect", "ProphecyResultAspect" });

                if (relevantListNonDrop.Count <= 0) return false;
                if (relevantListRest.Count <= 0) return true;
                return !relevantListRest.Max(x => x.CVal > FilterPolishConfig.UniqueT2BreakPoint * 0.5f);
            });

            builder.AddRule("hideable", "hideable", s =>
                {
                    var relevantList = builder.Item.AspectCheck(new HashSet<string>() { }, new HashSet<string>() { "NonDropAspect", "PreventHidingAspect", "ProphecyResultAspect" });

                    if (relevantList.Count == 0)
                    {
                        return false;
                    }

                    var maxprice = relevantList.Max(x => x.CVal);
                    if (maxprice > FilterPolishConfig.UniqueT2BreakPoint * 0.35f)
                    {
                        return false;
                    }

                    if (builder.Item.AllItemsFullFill(new HashSet<string>() { }, new HashSet<string>(){ "HighVarietyAspect", "LeagueDropAspect", "NonEventDropAspect", "BossDropAspect", "UncommonAspect", "PreventHidingAspect" }))
                    {
                        return true;
                    }

                    return false;
                });

            builder.AddExplicitRest("t3", "t3");

            return builder.Build();
        }
    }
}
