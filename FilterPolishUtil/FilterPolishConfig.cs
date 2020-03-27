﻿using FilterPolishUtil.Model;
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
            "uniques", "divination", "currency", "currency->deliriumorbs", "fragments", "unique->maps", "rare->shaper", "rare->elder", "rare->hunter", "rare->crusader", "rare->redeemer", "rare->warlord", "generalcrafting", "normalcraft->i86", "currency->fossil", "currency->incubators", "currency->prophecy", "fragments->scarabs", "currency->oil", "vials"
        };

        /// <summary>
        /// These bases get special treatment, because of their special drop location and properties.
        /// </summary>
        public static HashSet<string> SpecialBases { get; set; } = new HashSet<string>()
        {
            "Opal Ring", "Steel Ring", "Vermillion Ring", "Blue Pearl Amulet", "Bone Helmet", "Cerulean Ring", "Convoking Wand", "Crystal Belt", "Fingerless Silk Gloves", "Gripped Gloves", "Marble Amulet", "Sacrificial Garb", "Spiked Gloves", "Stygian Vise", "Two-Toned Boots", "Vanguard Belt"
        };

        /// <summary>
        /// Saves, caches and uses requested files if active.
        /// </summary>
        public static RequestType ActiveRequestMode = RequestType.Dynamic;

        public static ExecutionMode ApplicationExecutionMode = ExecutionMode.OnPrem;

        /// <summary>
        /// Information about the requests and economyoverview sections:
        /// name, filename, requesturl, urlprefix. Probably should've added a class for that.
        /// </summary>
        public static List<Tuple<string, string, string>> FileRequestData { get; set; } = new List<Tuple<string, string, string>>
        {
                new Tuple<string, string, string>("currency", "currency", "https://poe.ninja/api/data/currencyoverview?type=Currency"),
                new Tuple<string, string, string>("fragments", "fragments", "https://poe.ninja/api/data/currencyoverview?type=Fragment"),
                new Tuple<string, string, string>("divination", "divination", "https://poe.ninja/api/data/itemoverview?type=DivinationCard"),
                new Tuple<string, string, string>("unique->maps", "uniqueMaps", "https://poe.ninja/api/data/itemoverview?type=UniqueMap"),
                new Tuple<string, string, string>("currency->fossil", "fossil", "https://poe.ninja/api/data/itemoverview?type=Fossil"),
                new Tuple<string, string, string>("uniques", "uniqueWeapons", "https://poe.ninja/api/data/itemoverview?type=UniqueWeapon"),
                new Tuple<string, string, string>("uniques", "uniqueFlasks", "https://poe.ninja/api/data/itemoverview?type=UniqueFlask"),
                new Tuple<string, string, string>("uniques", "uniqueArmours", "https://poe.ninja/api/data/itemoverview?type=UniqueArmour"),
                new Tuple<string, string, string>("uniques", "uniqueAccessory", "https://poe.ninja/api/data/itemoverview?type=UniqueAccessory"),
                new Tuple<string, string, string>("basetypes", "basetypes", "https://poe.ninja/api/data/itemoverview?type=BaseType"),
                new Tuple<string, string, string>("currency->incubators", "incubators", "https://poe.ninja/api/data/itemoverview?type=Incubator"),
                new Tuple<string, string, string>("currency->prophecy", "prophecy", "https://poe.ninja/api/data/itemoverview?type=Prophecy"),
                new Tuple<string, string, string>("fragments->scarabs", "scarabs", "https://poe.ninja/api/data/itemoverview?type=Scarab"),
                new Tuple<string, string, string>("currency->oil", "oil", "https://poe.ninja/api/data/itemoverview?type=Oil"),
                new Tuple<string, string, string>("vials", "vials", "https://poe.ninja/api/data/itemoverview?type=Vial"),
                new Tuple<string, string, string>("currency->deliriumorbs", "deliriumorbs", "https://poe.ninja/api/data/itemoverview?type=DeliriumOrb")
        };

        /// <summary>
        /// Aspects that should be ignored during certain enrichments procedures
        /// </summary>
        public static List<string> GlobalIgnoreAspects = new List<string>() { "IgnoreAspect" };
        public static List<string> IgnoredHighestPriceAspects { get; } = new List<string>() { "ProphecyResultAspect", "NonDropAspect" };
        public static List<string> IgnoredLowestPriceAspects { get; } = new List<string>() { "ProphecyResultAspect", "NonDropAspect", "BossDropAspect", "LeagueDropAspect" };

        /// <summary>
        /// Usually higher droplevel = better (to a degree), these bases are the exception, since they follow different distributions.
        /// </summary>
        public static HashSet<string> DropLevelIgnoredClasses = new HashSet<string>()
        {
            "rings", "amulets", "belts", "jewels", "rune dagger", "wands", "sceptres"
        };

        /// <summary>
        /// Incredibly important. Tells us a lot about the global economy, the freshness of the league and the crafting/gear demands.
        /// Adjusted during the first steps of enrichments procedures
        /// </summary>
        public static float ExaltedOrbPrice = 30f;

        /// <summary>
        /// By default no tiering is applied to a section if the exalted orb price is lower than this price, because the economy is usually not stable enough.
        /// </summary>
        public static float TieringEnablingExaltedOrbPrice = 35f;

        /// <summary>
        /// Useful for easy acquiring the section names above, without redundancy
        /// </summary>
        public static List<string> TierableEconomySections => FileRequestData.Select(x => x.Item1).Distinct().ToList();

        public static void AdjustPricingInformation()
        {
            SuperTierBreakPoint = 1f * ExaltedOrbPrice;

            // Uniques
            UniqueT1BreakPoint = UniqueT1Base + UniqueExaltedOrbInfluence * T1ExaltedInfluence * ExaltedOrbPrice;
            UniqueT2BreakPoint = UniqueT2Base + UniqueExaltedOrbInfluence * T2ExaltedInfluence * ExaltedOrbPrice;

            // BaseTypes
            BaseTypeT1BreakPoint = BaseTypeT1Base + BasesExaltedOrbInfluence * T1ExaltedInfluence * ExaltedOrbPrice;
            BaseTypeT2BreakPoint = BaseTypeT2Base + BasesExaltedOrbInfluence * T2ExaltedInfluence * ExaltedOrbPrice;

            // Divination
            DiviT1BreakPoint = DiviT1Base + DivinationExaltedOrbInfluence * T1ExaltedInfluence * ExaltedOrbPrice;
            DiviT2BreakPoint = DiviT2Base + DivinationExaltedOrbInfluence * T2ExaltedInfluence * ExaltedOrbPrice;
            DiviT3BreakPoint = DiviT3Base + DivinationExaltedOrbInfluence * T3ExaltedInfluence * ExaltedOrbPrice;
            DiviT5BreakPoint = DiviT5Base + DivinationExaltedOrbInfluence * T5ExaltedInfluence * ExaltedOrbPrice;

            // Misc
            MiscT1BreakPoint = MiscT1Base + MiscExaltedOrbInfluence * T1ExaltedInfluence * ExaltedOrbPrice;
            MiscT2BreakPoint = MiscT2Base + MiscExaltedOrbInfluence * T2ExaltedInfluence * ExaltedOrbPrice;
            MiscT3BreakPoint = MiscT3Base + MiscExaltedOrbInfluence * T3ExaltedInfluence * ExaltedOrbPrice;
            MiscT4BreakPoint = MiscT4Base + MiscExaltedOrbInfluence * T4ExaltedInfluence * ExaltedOrbPrice;

            // InfluenceGroups
            InfluenceGroupT1BreakPoint = InfluenceGroupT1Base + InfluenceGroupExaltedOrbInfluence * T1ExaltedInfluence * ExaltedOrbPrice;
            InfluenceGroupT2BreakPoint = InfluenceGroupT2Base + InfluenceGroupExaltedOrbInfluence * T2ExaltedInfluence * ExaltedOrbPrice;

            LoggingFacade.LogInfo($"Prices Adjusted based on exalted orb price!");
        }

        // Prices scale based on the exalted orb price. This mostly affects T1 prices
        public static float T1ExaltedInfluence = 0.70f;
        public static float T2ExaltedInfluence = 0.2f;
        public static float T3ExaltedInfluence = 0.05f;
        public static float T4ExaltedInfluence = 0.02f;
        public static float T5ExaltedInfluence = 0.008f;

        // Exception for league only, uncommon, special uniques. Currently set at 3.5ex, but could be higher easily.
        public static float SuperTierBreakPoint; 

        // Unique Breakpoints - uniques have a lower breakpoints, due to item stat variations
        public static float UniqueExaltedOrbInfluence = 0.1f;

        public static float UniqueT1Base = 20f;
        public static float UniqueT2Base = 5f;

        public static float UniqueT1BreakPoint;
        public static float UniqueT2BreakPoint;

        // Uniques have a bunch of special conditions, that are used as pricing multipliers
        public static float UncommonAspectMultiplier = 2f;    // Items with several versions and one uncommon version need to reach X the T2 breakpoint
        public static float CommonTwinAspectMultiplier = 1.25f;  // Items with several versions. The rare version needs to reach X the T2 breakpoint
        public static float LeagueDropAspectMultiplier = 6f;    // League Drop Items, that are NOT boss drops need to reach X the T2 breakpoint
        public static float HighVarietyMultiplier = 0.5f;       // Items like ventor that have crazy roll ranges need to reach a way lower min price

        // BaseType Pricing - basetype tiered items can be more expensive due to a plethora of different factors
        // However, to minimize disapointing mistakes, we keep the T1 breakpoint up high
        public static float BasesExaltedOrbInfluence = 0.12f;

        public static float BaseTypeT1Base = 25f;
        public static float BaseTypeT2Base = 6.5f;

        public static float BaseTypeT1BreakPoint;
        public static float BaseTypeT2BreakPoint;

        // Divination cards are tricky. You have to consider their special nature of not being useful until a set is complete and that it takes an extra overhead to purify their value.
        public static float DivinationExaltedOrbInfluence = 0.12f;

        public static float DiviT1BreakPoint;
        public static float DiviT2BreakPoint;
        public static float DiviT3BreakPoint;
        public static float DiviT5BreakPoint;

        public static float DiviT1Base = 25f;
        public static float DiviT2Base = 6.5f;
        public static float DiviT3Base = 2f;
        public static float DiviT5Base = 0.5f;

        // Fossils and scarabs are often predictable -drops-. Predictable drops are often best kept at high threshholds. Predictability ruins the surprise/excitement
        public static float MiscExaltedOrbInfluence = 0.05f;

        public static float MiscT1BreakPoint;
        public static float MiscT2BreakPoint;
        public static float MiscT3BreakPoint;
        public static float MiscT4BreakPoint;

        public static float MiscT1Base = 40f;
        public static float MiscT2Base = 8f;
        public static float MiscT3Base = 2.5f;
        public static float MiscT4Base = 0.75f;

        // ClassAbstractionConstants
        public static float InfluenceGroupExaltedOrbInfluence = 0.1f;

        public static float InfluenceGroupT1BreakPoint;
        public static float InfluenceGroupT2BreakPoint;

        public static float InfluenceGroupT1Base = 35f;
        public static float InfluenceGroupT2Base = 5f;
    }

    public enum RequestType
    {
        Dynamic,
        ForceOnline
    }

    public enum ExecutionMode
    {
        Function,
        OnPrem
    }
}
