using System;
using System.Collections.Generic;
using System.Text;

namespace FilterExo
{
    public static class FilterExoConfig
    {
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
            '=', '>', '<', '(', ')', '{', '}', '[', ']'
        };

        public static HashSet<string> CombinedOperators = new HashSet<string>()
        {
            "==", ">=", "<=", "<>", "=>"
        };
    }
}
