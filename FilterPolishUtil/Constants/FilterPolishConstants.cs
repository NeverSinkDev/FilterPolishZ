using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishUtil.Constants
{
    public static class FilterPolishConstants
    {
        public static Dictionary<string, string> Abbreviations { get; set; } = new Dictionary<string, string>()
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
        public static float T2BreakPoint = 8;
    }
}
