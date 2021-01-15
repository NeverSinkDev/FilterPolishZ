using System;
using System.Collections.Generic;
using System.Text;

namespace FilterExo
{
    public static class FilterExoConfig
    {
        // PROCESSOR

        public enum ExoFilterType
        {
            generic,
            root,
            comment
        }

        public enum ExoExpressionCommandSource
        {
            direct,
            mutator,
            style
        }

        // TOKENIZER

        public static HashSet<char> Separators = new HashSet<char>()
        {
            ' ',
            '\t'
        };

        public enum TokenizerMode
        {
            normal,
            comment,
            quoted
        }

        public static char QuoteCharacter = '\"';
        public static char CommentCharacter = '#';

        public static HashSet<char> SimpleOperators = new HashSet<char>()
        {
            '=', '>', '<', '(', ')', '{', '}', '[', ']', ';', ',', '+', '-'
        };

        public static HashSet<string> CombinedOperators = new HashSet<string>()
        {
            "==", ">=", "<=", "<>", "=>"
        };

        // STRUCTURIZER

        public enum StructurizerMode
        {
            root,
            atom,
            scop,
            comm
        }

        public enum StructurizerScopeType
        {
            none,
            impl,
            expl
        }

        public static Dictionary<string, string> Abbreviations = new Dictionary<string, string>()
        {
            { "Background", "SetBackgroundColor" },
            { "BG", "SetBackgroundColor" },

            { "Border", "SetBorderColor" },
            { "BC", "SetBorderColor" },
            { "BD", "SetBorderColor" },

            { "Text", "SetTextColor" },
            { "TX", "SetTextColor" },

            { "Font", "SetFontSize" },

            { "BT", "BaseType" },
            { "Base", "BaseType" },

            { "Mod", "HasExplicitMod" }
        };
    }
}
