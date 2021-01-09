using FilterCore.Commands.EntryCommands;
using FilterDomain.LineStrategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilterCore.Entry;

namespace FilterCore
{
    public class FilterGenerationConfig
    {
        static FilterGenerationConfig()
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
            { "Replica",       new BoolLineStrategy() },
            { "AlternateQuality",       new BoolLineStrategy() },
            { "FracturedItem",       new BoolLineStrategy() },
            { "SynthesisedItem",     new BoolLineStrategy() },
            { "AnyEnchantment",      new BoolLineStrategy() },
            { "BlightedMap",   new BoolLineStrategy() },
            { "ShapedMap",     new BoolLineStrategy() },
            { "ElderItem",     new BoolLineStrategy() },
            { "ShaperItem",    new BoolLineStrategy() },
            { "Corrupted",     new BoolLineStrategy() },
            { "Identified",    new BoolLineStrategy() },
            { "ElderMap",      new BoolLineStrategy() },
            { "Mirrored",      new BoolLineStrategy() },

            { "HasInfluence",  new EnumLineStrategy(true) },
            { "GemQualityType",new EnumLineStrategy(true) },
            { "LinkedSockets", new NumericLineStrategy() },
            { "Sockets",       new NumericLineStrategy() },
            { "Quality",       new NumericLineStrategy() },
            { "MapTier",       new NumericLineStrategy() },
            { "Width",         new NumericLineStrategy() },
            { "Height",        new NumericLineStrategy() },
            { "StackSize",     new NumericLineStrategy() },
            { "CorruptedMods", new NumericLineStrategy() },
            { "GemLevel",      new NumericLineStrategy() },
            { "ItemLevel",     new NumericLineStrategy() },
            { "DropLevel",     new NumericLineStrategy() },
            { "AreaLevel",     new NumericLineStrategy() },
            { "Rarity",        new NumericLineStrategy() },

            { "EnchantmentPassiveNum", new NumericLineStrategy() },

            { "SocketGroup",   new EnumLineStrategy(false) },
            { "Class",         new EnumLineStrategy(true) },
            { "BaseType",      new EnumLineStrategy(true) },
            { "Prophecy",      new EnumLineStrategy(true) },
            { "HasExplicitMod",new EnumLineStrategy(false) },

            { "SetFontSize",             new VariableLineStrategy(1,1) },
            { "SetTextColor",            new ColorLineStrategy() },
            { "SetBorderColor",          new ColorLineStrategy() },
            { "SetBackgroundColor",      new ColorLineStrategy() },
            { "DisableDropSound",        new VariableLineStrategy(0,1) },
            { "PlayAlertSound",          new VariableLineStrategy(1,2) },
            { "PlayAlertSoundPositional",new VariableLineStrategy(1,2) },
            { "CustomAlertSound",        new VariableLineStrategy(1,2) },
            { "PlayEffect",              new VariableLineStrategy(2,3) },
            { "MinimapIcon",             new VariableLineStrategy(3,3) },

            { "Continue",                new VariableLineStrategy(0,0) }};

        public static Dictionary<string, Func<FilterEntry, IEntryCommand>> EntryCommand = new Dictionary<string, Func<FilterEntry, IEntryCommand>>
        {
            { "D",      entry => new DisableEntryCommand(entry) },
            { "H",      entry => new HideEntryCommand(entry) },
            { "RF",     entry => new ReduceFontSizeEntryCommand(entry) },
            { "HS",     entry => new RemoveHighlightsThenHideEntryCommand(entry) },
            { "REMS",   entry => new RemoveHighlightsEntryCommand(entry) },
            { "UP",     entry => new RaresUpEntryCommand(entry) },
            { "RVR",    entry => new RarityVariationRuleEntryCommand(entry) },
            { "C",      entry => new ConsoleStrictnessCommand(entry) },
            { "SENDER", entry => new SenderEntryCommand(entry) },
            { "RECEIVER",entry => new ReceiverEntryCommand(entry) },
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
            "=", ">=", "<=", ">", "<", "=="
        };

        public static readonly List<string> FilterStrictnessLevels = new List<string>
        {
            "Soft", "Regular", "Semi-Strict", "Strict", "Very-Strict", "Uber-Strict", "Uber-Plus-Strict"
        };
        
        public static readonly Dictionary<string, List<string>> FilterStrictnessApiIds = new Dictionary<string, List<string>>
        {
            { "tmpstandard", new List<string> { "GlLZt1", "G4AtJ5", "GmjoTD", "5XMehq", "GmXoTD", "wvPeTJ", "Gg4zsE" } },
            { "tmphardcore", new List<string> { "WXjRcE", "0JX8H0", "M479i0", "8kvycE", "om2WSo", "kmDDcN", "DEgxfQ" } }
        };

        public static IEnumerable<string> FilterStyles; // later auto-generated by reading all styleSheet files
        
        // all appearance related idents that should be removed on certain strictness levels and when hiding items
        public static readonly HashSet<string> HighlightingIdents = new HashSet<string>
        {
            "PlayEffect", "MinimapIcon", "PlayAlertSound", "CustomAlertSound", "PlayAlertSoundPositional"
        };

        public static readonly HashSet<string> ValidRarities = new HashSet<string>
        {
            "Normal", "Magic", "Rare", "Unique"
        };
    }
}
