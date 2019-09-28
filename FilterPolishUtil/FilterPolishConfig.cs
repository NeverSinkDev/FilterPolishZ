using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishUtil
{
    public static class FilterPolishConfig
    {
        /// <summary>
        /// These are the in-filter tierlists we parse, denoted by the $Type->... comment
        /// It's different from the FileRequestData below, because it contains derived sections such as rare->elder
        /// </summary>
        public static HashSet<string> FilterTierLists { get; set; } = new HashSet<string>()
        {
            "uniques", "divination", "unique->maps", "rare->shaper", "rare->elder", "generalcrafting", "normalcraft->i86", "currency->fossil", "currency->incubators", "currency->prophecy", "fragments->scarabs", "currency->oil"
        };

        public static HashSet<string> SpecialBases { get; set; } = new HashSet<string>()
        {
            "Opal Ring", "Steel Ring", "Vermillion Ring", "Blue Pearl Amulet", "Bone Helmet", "Cerulean Ring", "Convoking Wand", "Crystal Belt", "Fingerless Silk Gloves", "Gripped Gloves", "Marble Amulet", "Sacrificial Garb", "Spiked Gloves", "Stygian Vise", "Two-Toned Boots", "Vanguard Belt"
        };

        /// <summary>
        /// Saves, caches and uses requested files if active.
        /// </summary>
        public static RequestType ActiveRequestMode = RequestType.Dynamic;

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
                new Tuple<string, string, string, string>("fragments->scarabs", "scarabs", "ItemOverview?type=Scarab", "&"),
                new Tuple<string, string, string, string>("currency->oil", "oil", "ItemOverview?type=Oil", "&")
        };

        /// <summary>
        /// Useful for easy acquiring the section names above, without redundancy
        /// </summary>
        public static List<string> TierableEconomySections => FileRequestData.Select(x => x.Item1).Distinct().ToList();

        public static float GlobalPriceModifier = 0.8f;

        public static float SuperTierBreakPoint = 600 * GlobalPriceModifier; // Exception for league only, uncommon, special uniques

        // Unique Breakpoints - uniques have a lower breakpoints, due to item stat variations
        public static float T1BreakPoint = 20 * GlobalPriceModifier;
        public static float T2BreakPoint = 5 * GlobalPriceModifier;

        // BaseType Pricing - basetype tiered items can be more expensive due to a plethora of different factors
        // However, to minimize disapointing mistakes, we keep the T1 breakpoint up high
        public static float T1BaseTypeBreakPoint = 35f * GlobalPriceModifier;
        public static float T2BaseTypeBreakPoint = 6f * GlobalPriceModifier;

        // Divination cards are tricky
        public static float T1DiviBreakPoint = 25f * GlobalPriceModifier;
        public static float T2DiviBreakPoint = 8f * GlobalPriceModifier;
        public static float T3DiviBreakPoint = 2f * GlobalPriceModifier;
        public static float T5DiviBreakPoint = 0.5f;

        // Fossils and scarabs are often predictable -drops-. Predictable drops are often best kept at high threshholds
        public static float T1MiscBreakPoint = 35f * GlobalPriceModifier;
        public static float T2MiscBreakPoint = 10f * GlobalPriceModifier;
        public static float T3MiscBreakPoint = 3f * GlobalPriceModifier;
        public static float T4MiscBreakPoint = 1f;

        public static float UncommonAspectMultiplier = 2.5f;    // Items with several versions and one uncommon version need to reach X the T2 breakpoint
        public static float CommonTwinAspectMultiplier = 1.5f;  // Items with several versions. The rare version needs to reach X the T2 breakpoint
        public static float LeagueDropAspectMultiplier = 6f;    // League Drop Items, that are NOT boss drops need to reach X the T2 breakpoint
        public static float HighVarietyMultiplier = 0.5f;       // Items like ventor that have crazy roll ranges need to reach a way lower min price
    }

    public enum RequestType
    {
        Dynamic,
        ForceOnline
    }
}
