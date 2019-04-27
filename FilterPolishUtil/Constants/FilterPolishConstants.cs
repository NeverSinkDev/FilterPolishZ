using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishUtil.Constants
{
    public static class FilterPolishConstants
    {
        /// <summary>
        /// These are the in-filter tierlists we parse, denoted by the $Type->... comment
        /// It's different from the FileRequestData below, because it contains derived sections such as rare->elder
        /// </summary>
        public static HashSet<string> FilterTierLists { get; set; } = new HashSet<string>()
        {
            "uniques", "divination", "unique->maps", "rare->shaper", "rare->elder", "currency->fossil"
        };

        /// <summary>
        /// Information about the requests and economyoverview sections:
        /// name, filename, requesturl, urlprefix
        /// </summary>
        public static List<Tuple<string, string, string, string>> FileRequestData { get; set; } = new List<Tuple<string, string, string, string>>
        {
                // 
                new Tuple<string, string, string, string>("divination", "divination", "GetDivinationCardsOverview", "?"),
                new Tuple<string, string, string, string>("unique->maps", "uniqueMaps", "GetUniqueMapOverview", "?"),
                new Tuple<string, string, string, string>("currency->fossil", "fossil", "ItemOverview?type=Fossil", "&"),
                new Tuple<string, string, string, string>("uniques", "uniqueWeapons", "GetUniqueWeaponOverview", "?"),
                new Tuple<string, string, string, string>("uniques", "uniqueFlasks", "GetUniqueFlaskOverview", "?"),
                new Tuple<string, string, string, string>("uniques", "uniqueArmours", "GetUniqueArmourOverview", "?"),
                new Tuple<string, string, string, string>("uniques", "uniqueAccessory", "GetUniqueAccessoryOverview", "?"),
                new Tuple<string, string, string, string>("basetypes", "basetypes", "ItemOverview?type=BaseType", "&")
        };

        /// <summary>
        /// Useful for easy acquiring the section names above, without redundancy
        /// </summary>
        public static List<string> TierableEconomySections => FileRequestData.Select(x => x.Item1).Distinct().ToList();

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
