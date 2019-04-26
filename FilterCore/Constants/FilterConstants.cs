using FilterCore.Commands.EntryCommands;
using FilterDomain.LineStrategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterCore.Constants
{
    public class FilterConstants
    {
        static FilterConstants()
        {
            short i = 0;
            foreach (var keys in LineTypes)
            {
                LineTypesSort.Add(keys.Key, i);
                i++;
            }

            short j = 0;
            foreach (var item in TierTagTypes)
            {
                TierTagSort.Add(item, j);
                j++;
            }

            LineTypesSort.Add("", i);
            i++;
            LineTypesSort.Add("comment", i);
        }

        public static List<string> GlobalIgnoreAspects = new List<string>() { "IgnoreAspect" };
        public static List<string> IgnoredHighestPriceAspects { get; } = new List<string>() { "ProphecyResultAspect", "NonDropAspect" };
        public static List<string> IgnoredLowestPriceAspects { get; } = new List<string>() { "ProphecyResultAspect", "NonDropAspect", "BossDropAspect", "LeagueDropAspect" };

        public static List<string> TierTagTypes = new List<string>()
        {
            "type",
            "tier",
            "league",
            "id",
            "size",
            "lvl",
            "eg",
            "intent",
            "x"
        };

        /// <summary>
        /// Lists all known FilterLine identifiers, their treatment strategy and the sort order.
        /// </summary>
        public static Dictionary<string, ILineStrategy> LineTypes = new Dictionary<string, ILineStrategy>(){
            { "Show",          new VariableLineStrategy(0,0) },
            { "Hide",          new VariableLineStrategy(0,0) },
             
            { "FracturedItem",       new BoolLineStrategy() },
            { "SynthesisedItem",     new BoolLineStrategy() },
            { "AnyEnchantment",      new BoolLineStrategy() },
            { "ShapedMap",     new BoolLineStrategy() },
            { "ElderItem",     new BoolLineStrategy() },
            { "ShaperItem",    new BoolLineStrategy() },
            { "Corrupted",     new BoolLineStrategy() },
            { "Identified",    new BoolLineStrategy() },
            { "ElderMap",      new BoolLineStrategy() },
            { "LinkedSockets", new NumericLineStrategy() },
            { "Sockets",       new NumericLineStrategy() },
            { "Quality",       new NumericLineStrategy() },
            { "MapTier",       new NumericLineStrategy() },
            { "Width",         new NumericLineStrategy() },
            { "Height",        new NumericLineStrategy() },
            { "StackSize",     new NumericLineStrategy() },
            { "GemLevel",      new NumericLineStrategy() },
            { "ItemLevel",     new NumericLineStrategy() },
            { "DropLevel",     new NumericLineStrategy() },
            { "Rarity",        new NumericLineStrategy() },
            { "SocketGroup",   new EnumLineStrategy() },
            { "Class",         new EnumLineStrategy() },
            { "BaseType",      new EnumLineStrategy() },
            { "Prophecy",      new EnumLineStrategy() },
            { "HasExplicitMod",new EnumLineStrategy() },

            { "SetFontSize",             new VariableLineStrategy(1,1) },
            { "SetTextColor",            new ColorLineStrategy() },
            { "SetBorderColor",          new ColorLineStrategy() },
            { "SetBackgroundColor",      new ColorLineStrategy() },
            { "DisableDropSound",        new VariableLineStrategy(0,1) },
            { "PlayAlertSound",          new VariableLineStrategy(1,2) },
            { "PlayAlertSoundPositional",new VariableLineStrategy(1,2) },
            { "CustomAlertSound",        new VariableLineStrategy(1,2) },
            { "PlayEffect",              new VariableLineStrategy(2,3) },
            { "MinimapIcon",             new VariableLineStrategy(3,3) }};

        public static Dictionary<string, Type> EntryCommand = new Dictionary<string, Type>()
        {
            { "D",      typeof(DisableEntryCommand) },
            { "H",      typeof(HideEntryCommand) },
            { "RF",     typeof(ReduceFontSizeEntryCommand) },
            { "HS",     typeof(RemoveHighlightsThenHideEntryCommand) },
            { "REMS",   typeof(RemoveHighlightsEntryCommand) },
            { "UP",     typeof(RaresUpEntryCommand) },
            { "RVR",    typeof(RarityVariationRuleEntryCommand) }
        };

        public static HashSet<string> StyleIdents = new HashSet<string>
        {
            "SetTextColor",
            "SetBorderColor",
            "SetBackgroundColor",
            "PlayAlertSound"
        };

        public static char[] WhiteLineChars = new char[] { ' ', '\t' };

        /// <summary>
        /// A quick access to the sort order of the filterline idents
        /// </summary>
        public static Dictionary<string, int> LineTypesSort = new Dictionary<string, int>();
        public static Dictionary<string, int> TierTagSort = new Dictionary<string, int>();

        public static HashSet<string> IgnoredSuggestionTiers = new HashSet<string> { "rest", "???" };

        public enum FilterEntryType
        {
            Content,
            Filler,
            Comment,
            Unknown
        }

        public enum FilterLineIdentPhase
        {
            None,
            IdentScan,
            CommentScan,
            ValueScan,
            ValueCommentScan,
            ValueTagScan
        }

        public static HashSet<string> FilterOperators = new HashSet<string>()
        {
            "=", ">=", "<=", ">", "<"
        };

        public static HashSet<string> DropLevelIgnoredClasses = new HashSet<string>()
        {
            "rings", "amulets", "belts", "jewels", "daggers", "wands", "sceptres"
        };

        public static readonly List<string> FilterStrictnessLevels = new List<string>
        {
            "Soft", "Regular", "Semi-Strict", "Strict", "Very-Strict", "Uber-Strict", "Uber-Plus-Strict"
        };

        public static IEnumerable<string> FilterStyles; // later auto-generated by reading all styleSheet files
        
        // all appearance related idents that should be removed on certain strictness levels and when hiding items
        public static readonly HashSet<string> HighlightingIdents = new HashSet<string>
        {
            "PlayEffect", "MinimapIcon", "PlayAlertSound", "CustomAlertSound", "PlayAlertSoundPositional"
        };
    }
}
