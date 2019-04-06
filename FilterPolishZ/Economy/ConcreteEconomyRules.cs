using FilterCore.FilterComponents.Tier;
using FilterEconomy.Facades;
using FilterEconomy.Model;
using FilterEconomy.Processor;
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
                if (TierListFacade.ContainsTierInformationForBaseType("uniques", item.BaseType))
                {
                    item.OldTier = TierListFacade.GetTiersForBasetype("uniques", item.BaseType).First();
                }
            }

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
                    z => z.Aspects.Any(x => include.Contains(x.Name)) &&
                         z.Aspects.All(x => !exclude.Contains(x.Name))).ToList();

            // Anchor item
            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
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
                TargetTier = "unknown",

                Rule = (string s) =>
                {
                    return !EconomyInformation.EconomyTierlistOverview["uniques"].ContainsKey(s);
                }
            });

            // T1 unique
            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                TargetTier = "t1",
                Rule = (string s) =>
                {
                    return uniqueRules.DefaultSet.LowestPrice > FilterPolishConstants.T1BreakPoint;
                }
            });

            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                TargetTier = "t2",
                Rule = (string s) =>
                {
                    return uniqueRules.DefaultSet.LowestPrice > FilterPolishConstants.T2BreakPoint;
                }
            });

            uniqueRules.EconomyRules.Add(new FilterEconomyRule()
            {
                TargetTier = "uncommon",
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
                TargetTier = "highVariety",
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
                TargetTier = "LeagueDropAspect",
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
                TargetTier = "Prophecy",
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
                TargetTier = "Rest",
                Rule = (string s) =>
                {
                    return true;
                }
            });

            return uniqueRules;
        }
    }
}
