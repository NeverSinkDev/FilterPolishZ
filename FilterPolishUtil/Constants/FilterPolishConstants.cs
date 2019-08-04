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
            "uniques", "divination", "unique->maps", "rare->shaper", "rare->elder", "rare->normal", "currency->fossil", "currency->incubators", "currency->prophecy", "fragments->scarabs"
        };

        /// <summary>
        /// Information about the requests and economyoverview sections:
        /// name, filename, requesturl, urlprefix
        /// </summary>
        public static List<Tuple<string, string, string, string>> FileRequestData { get; set; } = new List<Tuple<string, string, string, string>>
        {
                new Tuple<string, string, string, string>("divination", "divination", "GetDivinationCardsOverview", "?"),
                new Tuple<string, string, string, string>("unique->maps", "uniqueMaps", "GetUniqueMapOverview", "?"),
                new Tuple<string, string, string, string>("currency->fossil", "fossil", "ItemOverview?type=Fossil", "&"),
                new Tuple<string, string, string, string>("uniques", "uniqueWeapons", "GetUniqueWeaponOverview", "?"),
                new Tuple<string, string, string, string>("uniques", "uniqueFlasks", "GetUniqueFlaskOverview", "?"),
                new Tuple<string, string, string, string>("uniques", "uniqueArmours", "GetUniqueArmourOverview", "?"),
                new Tuple<string, string, string, string>("uniques", "uniqueAccessory", "GetUniqueAccessoryOverview", "?"),
                new Tuple<string, string, string, string>("basetypes", "basetypes", "ItemOverview?type=BaseType", "&"),
                new Tuple<string, string, string, string>("currency->incubators", "incubators", "ItemOverview?type=Incubator", "&"),
                new Tuple<string, string, string, string>("currency->prophecy", "prophecy", "ItemOverview?type=Prophecy", "&"),
                new Tuple<string, string, string, string>("fragments->scarabs", "scarabs", "ItemOverview?type=Scarab", "&")
        };

        /// <summary>
        /// Useful for easy acquiring the section names above, without redundancy
        /// </summary>
        public static List<string> TierableEconomySections => FileRequestData.Select(x => x.Item1).Distinct().ToList();

        public static float SuperTierBreakPoint = 600;          // Exception for league only, uncommon, special uniques

        public static float T1BreakPoint = 20;
        public static float T2BreakPoint = 5;

        public static float T1BaseTypeBreakPoint = 25f;
        public static float T2BaseTypeBreakPoint = 6f;

        public static float T1DiviBreakPoint = 20f;
        public static float T2DiviBreakPoint = 9f;
        public static float T3DiviBreakPoint = 2f;
        public static float T5DiviBreakPoint = 0.5f;

        public static float UncommonAspectMultiplier = 2.5f;    // Items with several versions and one uncommon version need to reach X the T2 breakpoint
        public static float CommonTwinAspectMultiplier = 1.5f;  // Items with several versions. The rare version needs to reach X the T2 breakpoint
        public static float LeagueDropAspectMultiplier = 6f;    // League Drop Items, that are NOT boss drops need to reach X the T2 breakpoint
        public static float HighVarietyMultiplier = 0.5f;       // Items like ventor that have crazy roll ranges need to reach a way lower min price




    }
}
