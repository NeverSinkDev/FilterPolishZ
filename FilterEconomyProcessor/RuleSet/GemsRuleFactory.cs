﻿using FilterEconomy.Model;
using FilterEconomy.Processor;
using FilterPolishUtil;
using System.Collections.Generic;
using System.Linq;

namespace FilterEconomyProcessor.RuleSet
{
    public static class GemsRuleFactory
    {
        public static HashSet<string> ExceptionGems =
            new HashSet<string>() {"Empower Support", "Enlighten Support", "Enhance Support"};

        public static FilterEconomyRuleSet Generate(ConcreteEconomyRules ruleHost)
        {
            var builder = new RuleSetBuilder(ruleHost).SetSection("gems").UseDefaultQuery().AddDefaultPostProcessing()
                .SkipInEarlyLeague().AddDefaultIntegrationTarget();
            builder.AddRule("t1-awa", x => IsAwakened() && MinIst1(1.25f));
            builder.AddRule("t1-ano", x => IsAltQuality("anomalous") && MinIst1());
            builder.AddRule("t1-div", x => IsAltQuality("divergent") && MinIst1());
            builder.AddRule("t1-pha", x => IsAltQuality("phantasmal") && MinIst1());
            builder.AddRule("exception", "rest", x => ExceptionGems.Contains(builder.Item[0].Name));

            // t1 LEVEL RULES
            // 20-20, no corruption
            builder.AddRule("t1-20-20z", x => GemQuery(20, 20, false, 2, FilterPolishConfig.GemT1BreakPoint), nextgroup: "t2");
            builder.AddRule("t1-21-00", x => GemQuery(21, 0, null, 2, FilterPolishConfig.GemT1BreakPoint * 1.2f), nextgroup: "t2");
            builder.AddRule("t1-21-20", x => GemQuery(21, 20, null, 2, FilterPolishConfig.GemT1BreakPoint * 1.2f), nextgroup: "t2");
            builder.AddRule("t1-21-23", x => GemQuery(21, 23, null, 2, FilterPolishConfig.GemT1BreakPoint * 1.2f), nextgroup: "t2");

            // t2 RULES !!!
            builder.AddRule("t2-19-00z", x => GemQuery(19, 0, false, 3, FilterPolishConfig.GemT2BreakPoint * 1f), group: "t2", nextgroup: "t2q");
            builder.AddRule("t2-19-19z", x => GemQuery(19, 19, false, 2, FilterPolishConfig.GemT2BreakPoint * 1f), group: "t2", nextgroup: "t2q");
            builder.AddRule("t2-20-00z", x => GemQuery(20, 0, false, 2, FilterPolishConfig.GemT2BreakPoint * 1f), group: "t2", nextgroup: "t2q");
            builder.AddRule("t2-21-00", x => GemQuery(21, 0, null, 2, FilterPolishConfig.GemT2BreakPoint * 1.2f), group: "t2", nextgroup: "t2q");
            builder.AddRule("t2-21-20", x => GemQuery(21, 20, null, 2, FilterPolishConfig.GemT2BreakPoint * 1.2f), group: "t2", nextgroup: "t2q");
            builder.AddRule("t2-21-23", x => GemQuery(21, 23, null, 2, FilterPolishConfig.GemT2BreakPoint * 1.2f), group: "t2", nextgroup: "t2q");

            // t2 QUALITY CHECK
            // 1-15, no corruption
            builder.AddRule("t2-01-15z", x => GemQuery(1, 15, false, 3, FilterPolishConfig.GemT2BreakPoint * 1.2f), group: "t2q");
            builder.AddRule("t2-01-20z", x => GemQuery(1, 20, false, 3, FilterPolishConfig.GemT2BreakPoint * 1.2f), group: "t2q");
            builder.AddRule("t2-01-23z", x => GemQuery(1, 23, false, 3, FilterPolishConfig.GemT2BreakPoint * 1.2f), group: "t2q");

            builder.AddRestRule();

            return builder.Build();

            // local functions
            bool MinIst1(float multiplier = 1f) =>
                builder.Item.LowestPrice > FilterPolishConfig.GemT1BreakPoint * multiplier;

            bool IsAwakened() => builder.Item[0].Name.ToLower().Contains("awakened");

            // runs the GemCheck algorith (below) and also filters out awakened, alt quality gems and checks that the indexedcount and price is appropriate
            bool GemQuery(int maxLevel, int maxQual, bool? corruptionState, int indexedCountMin, float minPrice)
            {
                if (IsAltQuality() || IsAwakened()) return false;
                var gems = GemCheck(maxLevel, maxQual, corruptionState);
                if (gems.Count > 0)
                {
                    var appropriatePrice = gems.Where(y => y.CVal > minPrice && y.IndexedCount >= indexedCountMin)
                        .ToList();
                    if (appropriatePrice.Count > 0) { return true; }
                }

                return false;
            }

            // gets a subset of a gems with quality, level and corruption states lower or equal to the provided data
            List<NinjaItem> GemCheck(int maxLevel = 1, int maxQuality = 0, bool? corruptState = null)
            {
                var checkedItems = builder.Item.Where(x =>
                {
                    int lvl, qual;
                    bool qualS, lvlS = true;
                    var corrupted = x.Variant.Contains("c");
                    var parts = x.Variant.Split('/').ToList();
                    if (parts.Count == 1)
                    {
                        lvlS = int.TryParse(parts[0], out lvl);
                        qualS = true;
                        qual = 0;
                    }
                    else
                    {
                        lvlS = int.TryParse(parts[0], out lvl);
                        qualS = int.TryParse(parts[1].Replace("c", ""), out qual);
                    }

                    // parsing error
                    if (!lvlS || !qualS) { return false; }

                    if (lvl > maxLevel || qual > maxQuality) return false;
                    if (corruptState == null) return true;
                    if (corruptState != corrupted) return false;
                    return true;
                }).ToList();
                return checkedItems;
            }

            bool IsAltQuality(string type = default)
            {
                var name = builder.Item[0].Name.ToLower();
                if (type != default) { return name.Contains(type); }

                var altQualities = new HashSet<string>() {"anomalous", "divergent", "phantasmal"};
                return altQualities.Any(item => name.Contains(item));
            }

            string GetAltQuality()
            {
                var altQualities = new HashSet<string>() {"anomalous", "divergent", "phantasmal"};
                var name = builder.Item[0].Name.ToLower();
                foreach (var item in altQualities)
                {
                    if (name.Contains(item)) return item;
                }

                return string.Empty;
            }
        }
    }
}