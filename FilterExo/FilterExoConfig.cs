using FilterExo.Core.Process.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilterExo
{
    public static class FilterExoConfig
    {
        // PROCESSOR
        public static Dictionary<string, IEXC> ExoCommandDict = new Dictionary<string, IEXC>()
        {
            { "Rule", new RuleIEXC() }
        };

        public enum ExoFilterType
        {
            generic,
            root,
            bearer,
            scope
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
            '=', '>', '<', '(', ')', '{', '}', '[', ']', ';'
        };

        public static HashSet<string> CombinedOperators = new HashSet<string>()
        {
            "==", ">=", "<=", "<>", "=>"
        };

        // STRUCTURIZER

        public enum StructurizerMode
        {
            root,
            expr,
            desc,
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
    }
}
