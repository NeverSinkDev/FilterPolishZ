using FilterCore.FilterComponents.Tier;
using FilterEconomy;
using FilterEconomy.Facades;
using FilterEconomy.Model;
using FilterEconomy.Processor;
using FilterPolishUtil;
using FilterPolishUtil.Constants;
using FilterUtilModels.Economy;
using System.Collections.Generic;
using System.Linq;

namespace FilterPolishZ.Economy
{
    public class ConcreteEconomyRules : IEconomyProcessorData
    {
        private FilterEconomyRuleSet uniqueRules;
        private List<TieringCommand> suggestions;

        public ItemInformationFacade ItemInformation { get; set; }
        public EconomyRequestFacade EconomyInformation { get; set; }
        public Dictionary<string, TierGroup> TierInformation { get; set; }
        public TierListFacade TierListFacade { get; set; }

        public ConcreteEconomyRules()
        {
            this.uniqueRules = this.CreateUniqueEconomyRules();
            this.EconomyInformation = EconomyRequestFacade.GetInstance();
            this.ItemInformation = ItemInformationFacade.GetInstance();
            this.TierListFacade = TierListFacade.GetInstance();
        }

        public void Execute()
        {
            this.suggestions = EconomyInformation.EconomyTierlistOverview["uniques"].Select(z => z.Key).Select(x => this.uniqueRules.ProcessItem("uniques", x, x, this)).ToList();

            foreach (var item in suggestions)
            {
                item.Group = "uniques";
                if (TierListFacade.ContainsTierInformationForBaseType("uniques", item.BaseType))
                {
                    item.OldTier = TierListFacade.GetTiersForBasetype("uniques", item.BaseType).First().SubStringLast("->");
                }
                else
                {
                    item.OldTier = "rest";
                }
            }
            
            this.TierListFacade.Suggestions["uniques"].AddRange(this.suggestions);
        }

        private FilterEconomyRuleSet CreateUniqueEconomyRules()
        {
            FilterEconomyRuleSet uniqueRules = new FilterEconomyRuleSet() { GoverningSection = "uniques" };
            uniqueRules.DefaultItemQuery = new System.Func<string, FilterPolishUtil.Collections.ItemList<FilterEconomy.Model.NinjaItem>>((s) => EconomyInformation.EconomyTierlistOverview["uniques"][s]);

            // Test for Aspect
            bool HasAspect(string s) => uniqueRules.DefaultSet.Any(z => z.Aspects.Any(j => j.Name == s));

            List<NinjaItem> OfAspect(string s) => uniqueRules.DefaultSet.Where(z => z.Aspects.Any(j => j.Name == s)).ToList();

            List<NinjaItem> AspectCheck(HashSet<string> include, HashSet<string> exclude) =>
                uniqueRules.DefaultSet.Where(
                    z => z.Aspects.Any(x => include.Contains(x.Name) || include.Count == 0) &&
                         z.Aspects.All(x => !exclude.Contains(x.Name))).ToList();

            bool AllItemsFullFill(List<NinjaItem> list ,HashSet<string> include, HashSet<string> exclude) =>
                list.All(
                    z => z.Aspects.Any(x => include.Contains(x.Name) || include.Count == 0) &&
                         z.Aspects.All(x => !exclude.Contains(x.Name)));

            // Anchor item
            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                RuleName = "ANCHOR",
                TargetTier = "ANCHOR",
                Rule = (string s) =>
                {
                    return uniqueRules.DefaultSet.Select(x => x.Aspects).ToList()
                    .Any(z => z.Any(a => a.ToString() == "AnchorAspect"));
                }
            });

            // Unknown Unique
            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                RuleName = "unknown",
                TargetTier = "unknown",

                Rule = (string s) =>
                {
                    return !EconomyInformation.EconomyTierlistOverview["uniques"].ContainsKey(s);
                }
            });

            // T1 unique
            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                RuleName = "t1",
                TargetTier = "t1",
                Rule = (string s) =>
                {
                    return uniqueRules.DefaultSet.LowestPrice > FilterPolishConstants.T1BreakPoint;
                }
            });

            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                RuleName = "t2",
                TargetTier = "t2",
                Rule = (string s) =>
                {
                    return uniqueRules.DefaultSet.LowestPrice > FilterPolishConstants.T2BreakPoint;
                }
            });

            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                RuleName = "uncommon",
                TargetTier = "multispecial",
                Rule = (string s) =>
                {
                    var fit = false;
                    if (uniqueRules.DefaultSet.HighestPrice > FilterPolishConstants.T2BreakPoint)
                    {
                        if (HasAspect("UncommonAspect"))
                        {
                            var relevantList = AspectCheck(new HashSet<string>() { "UncommonAspect" }, new HashSet<string>() { "BossDropAspect", "NonDropAspect", "LeagueDropAspect" });

                            if (relevantList.Count > 0)
                            {
                                return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConstants.T2BreakPoint * FilterPolishConstants.UncommonAspectMultiplier;
                            }
                        }
                    }

                    return fit;
                }
            });

            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                RuleName = "highVariety",
                TargetTier = "multispecial",
                Rule = (string s) =>
                {
                    var fit = false;
                    if (uniqueRules.DefaultSet.HighestPrice > FilterPolishConstants.T2BreakPoint * FilterPolishConstants.HighVarietyMultiplier)
                    {
                        if (HasAspect("HighVarietyAspect"))
                        {
                            var relevantList = AspectCheck(new HashSet<string>() { "HighVarietyAspect" }, new HashSet<string>() { "BossDropAspect", "NonDropAspect", "LeagueDropAspect" });

                            if (relevantList.Count > 0)
                            {
                                return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConstants.T2BreakPoint * FilterPolishConstants.HighVarietyMultiplier;
                            }
                        }
                    }

                    return fit;
                }
            });

            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                RuleName = "leagueDropAspect",
                TargetTier = "multileague",
                Rule = (string s) =>
                {
                    var fit = false;
                    if (uniqueRules.DefaultSet.HighestPrice > FilterPolishConstants.T2BreakPoint)
                    {
                        if (HasAspect("LeagueDropAspect"))
                        {
                            var relevantList = AspectCheck(new HashSet<string>() { "LeagueDropAspect" }, new HashSet<string>() { "BossDropAspect", "NonDropAspect" });

                            if (relevantList.Count > 0)
                            {
                                return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConstants.T2BreakPoint * FilterPolishConstants.LeagueDropAspectMultiplier;
                            }
                        }
                    }

                    return fit;
                }
            });

            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                RuleName = "BossOnly",
                TargetTier = "multispecial",
                Rule = (string s) =>
                {
                    var fit = false;
                    if (uniqueRules.DefaultSet.HighestPrice > FilterPolishConstants.T2BreakPoint)
                    {
                        var relevantList = AspectCheck(new HashSet<string> { },new HashSet<string>() { "NonDropAspect" });

                        if (relevantList.Count > 0 && AllItemsFullFill(relevantList, new HashSet<string>() { "BossDropAspect" }, new HashSet<string>()))
                        {
                            return relevantList.OrderByDescending(x => x.CVal).First().CVal > FilterPolishConstants.T2BreakPoint * FilterPolishConstants.LeagueDropAspectMultiplier;
                        }
                    }

                    return fit;
                }
            });

            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                RuleName = "???",
                TargetTier = "???",
                Rule = (string s) =>
                {
                    var fit = false;
                    if (uniqueRules.DefaultSet.HighestPrice > FilterPolishConstants.T2BreakPoint)
                    {
                        if (HasAspect("LeagueDropAspect"))
                        {
                            return OfAspect("LeagueDropAspect").OrderByDescending(x => x.CVal).First().CVal > FilterPolishConstants.T2BreakPoint;
                        }
                    }

                    return fit;
                }
            });

            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                RuleName = "prophecy",
                TargetTier = "prophecy",
                Rule = (string s) =>
                {
                    var aspects = ItemInformation["uniques", s];
                    if (aspects == null)
                    {
                        return true;
                    }

                    return HasAspect("ProphecyMaterialAspect");
                }
            });

            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                RuleName = "rest",
                TargetTier = "rest",
                Rule = (string s) =>
                {
                    return true;
                }
            });

            return uniqueRules;
        }
    }
}
