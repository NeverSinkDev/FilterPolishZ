using FilterEconomy.Model;
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
            var builder = new RuleSetBuilder(ruleHost).SetSection("gems")
                .UseDefaultQuery()
                .AddDefaultPostProcessing()
                //.SkipInEarlyLeague()
                .AddDefaultIntegrationTarget();
            builder.AddRule("t1-awa", x => IsAwakened() && MinIst1(1.25f));


            builder.AddRule("rest", x =>
            {
                return (ruleHost.EconomyInformation.IsEarlyLeague());
            }, nextgroup: "t1");

            builder.AddRule("t1-ano", x => GemQuery(19, 20, null, 2, FilterPolishConfig.GemT1BreakPoint, "Anomalous"), group: "t1-ano", nextgroup: "ANY");
            builder.AddRule("t1-div", x => GemQuery(19, 20, null, 2, FilterPolishConfig.GemT1BreakPoint, "Divergent"), group: "t1-div", nextgroup: "ANY");
            builder.AddRule("t1-pha", x => GemQuery(19, 20, null, 2, FilterPolishConfig.GemT1BreakPoint, "Phantasmal"), group: "t1-pha", nextgroup: "ANY");
            builder.AddRule("exception", "rest", x => ExceptionGems.Contains(builder.Item[0].Name));

            // t1 LEVEL RULES
            // 20-20, no corruption
            builder.AddRule("t1-20-20z", x => GemQuery(20, 20, false, 2, FilterPolishConfig.GemT1BreakPoint), group: "t1");
            builder.AddRule("t1-21-20", x => GemQuery(21, 20, true, 2, FilterPolishConfig.GemT1BreakPoint), group: "t1");
            builder.AddRule("t1-21-23", x => GemQuery(21, 23, true, 2, FilterPolishConfig.GemT1BreakPoint), group: "t1");

            // t2 RULES !!!
            //builder.AddRule("t2-19-00z", x => GemQuery(19, 0, false, 3, FilterPolishConfig.GemT2BreakPoint * 1f), group: "t2", nextgroup: "t2q");
            //builder.AddRule("t2-19-19z", x => GemQuery(19, 19, false, 2, FilterPolishConfig.GemT2BreakPoint * 1f), group: "t2", nextgroup: "t2q");
            //builder.AddRule("t2-20-00z", x => GemQuery(20, 0, false, 2, FilterPolishConfig.GemT2BreakPoint * 1f), group: "t2", nextgroup: "t2q");
            //builder.AddRule("t2-21-00", x => GemQuery(21, 0, null, 2, FilterPolishConfig.GemT2BreakPoint * 1.2f), group: "t2", nextgroup: "t2q");
            //builder.AddRule("t2-21-20", x => GemQuery(21, 20, null, 2, FilterPolishConfig.GemT2BreakPoint * 1.2f), group: "t2", nextgroup: "t2q");
            //builder.AddRule("t2-21-23", x => GemQuery(21, 23, null, 2, FilterPolishConfig.GemT2BreakPoint * 1.2f), group: "t2", nextgroup: "t2q");

            // t2 QUALITY CHECK
            // 1-15, no corruption
            //builder.AddRule("t2-01-15z", x => GemQuery(1, 15, false, 3, FilterPolishConfig.GemT2BreakPoint * 1.2f), group: "t2q");
            //builder.AddRule("t2-01-20z", x => GemQuery(1, 20, false, 3, FilterPolishConfig.GemT2BreakPoint * 1.2f), group: "t2q");
            //builder.AddRule("t2-01-23z", x => GemQuery(1, 23, false, 3, FilterPolishConfig.GemT2BreakPoint * 1.2f), group: "t2q");

            builder.AddRestRule();

            return builder.Build();

            // local functions
            bool MinIst1(float multiplier = 1f) =>
                builder.Item.LowestPrice > FilterPolishConfig.GemT1BreakPoint * multiplier;

            bool IsAwakened() => builder.Item[0].Name.ToLower().Contains("awakened");

            // runs the GemCheck algorith (below) and also filters out awakened, alt quality gems and checks that the indexedcount and price is appropriate
            bool GemQuery(int maxLevel, int maxQual, bool? corruptionState, int indexedCountMin, float minPrice, string qualityType = "")
            {
                if (IsAwakened()) return false;

                var gems = GemCheck(maxLevel, maxQual, corruptionState, qualityType);
                if (gems.Count > 0)
                {
                    var appropriatePrice = gems.Where(y => y.CVal > minPrice && y.IndexedCount >= indexedCountMin)
                        .ToList();
                    if (appropriatePrice.Count > 0) { return true; }
                }

                return false;
            }

            // gets a subset of a gems with quality, level and corruption states lower or equal to the provided data
            List<NinjaItem> GemCheck(int maxLevel = 1, int maxQuality = 0, bool? corruptState = null, string qualityType = "")
            {
                var checkedItems = builder.Item.Where(x =>
                {
                    int lvl, qual;
                    bool qualS, lvlS = true;
                    var corrupted = x.Variant.Contains("c");
                    var parts = x.Variant.Split('/').ToList();
                    if (parts.Count == 1)
                    {
                        lvlS = int.TryParse(parts[0].Replace("c", ""), out lvl);
                        qualS = true;
                        qual = 0;
                    }
                    else
                    {
                        lvlS = int.TryParse(parts[0], out lvl);
                        qualS = int.TryParse(parts[1].Replace("c", ""), out qual);
                    }

                    if (x.EQualityType != qualityType) { return false; }

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
                if (type != default) { return builder.Item[0].EQualityType == type; }
                return builder.Item[0].EQualityType != string.Empty;
            }

            string GetAltQuality()
            {
                return builder.Item[0].EQualityType;
            }
        }
    }
}