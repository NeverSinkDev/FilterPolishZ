using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishUtil.Constants
{
    public static class FilterPolishConstants
    {
        public static Dictionary<string, List<string>> TieredGroups { get; } = new Dictionary<string, List<string>>()
        {
            {"divination",new List<string>(){"divination"}},
            {"unique->maps",new List<string>(){"uniqueMaps"}},
            {"uniques",new List<string>(){"uniqueWeapons","uniqueArmours","uniqueFlasks","uniqueAccessory"}},
            {"rare->shaper",new List<string>(){"rare->shaper"}},
            {"rare->elder",new List<string>(){"rare->elder"}},
            {"basetypes",new List<string>(){"basetypes"}}
        };

        public static Dictionary<string, string> Abbreviations { get; } = new Dictionary<string, string>()
        {
            { "divination", "GetDivinationCardsOverview" },
            { "basetypes", "ItemOverview?type=BaseType"},
            { "uniqueFlasks", "GetUniqueFlaskOverview" },
            { "uniqueWeapons", "GetUniqueWeaponOverview" },
            { "uniqueArmours", "GetUniqueArmourOverview"},
            { "uniqueAccessory", "GetUniqueAccessoryOverview" },
            { "uniqueMaps", "GetUniqueMapOverview" }
        };

        public static float T1BreakPoint = 20;
        public static float T2BreakPoint = 5;

        public static float UncommonAspectMultiplier = 3f;
        public static float LeagueDropAspectMultiplier = 6f;
        public static float HighVarietyMultiplier = 0.5f;

        public static float T1BaseTypeBreakPoint = 25f;
        public static float T2BaseTypeBreakPoint = 6f;

        public static float T5DiviBreakPoint = 0.5f;
        public static float T3DiviBreakPoint = 2f;
        public static float T2DiviBreakPoint = 9.5f;
        public static float T1DiviBreakPoint = 20f;

    }
}
