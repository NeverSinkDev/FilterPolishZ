using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FilterPolishUtil
{
    public static class EString
    {
        public static string Times(this string me, int count)
        {
            var res = "";

            for (var i = 0; i < count; i++)
            {
                res += me;
            }

            return res;
        }

        public static double ToDouble(this string s, double def = 0)
        {
            return string.IsNullOrEmpty(s) ? def : double.Parse(s);
        }

        public static string SubstringUntil(this string me, string until)
        {
            var loc = me.IndexOf(until, StringComparison.Ordinal);
            if (loc >= 0)
            {
                return me.Substring(0, loc);
            }
            return me;
        }

        public static string SubstringFrom(this string me, string from)
        {
            var loc = me.IndexOf(from, StringComparison.Ordinal);
            if (loc >= 0)
            {
                return me.Substring(loc + from.Length);
            }
            return me;
        }

        public static string ContainsAny(this string me, IEnumerable<string> comparisons)
        {
            var result = string.Empty;
            foreach (var comparison in comparisons)
            {
                if (me.Contains(comparison))
                {
                    if (result != string.Empty)
                    {
                        throw new Exception(
                            "Multiple Results Found for ContainsAny. Possibly Gem with Phantasmal in name?");
                    }
                    result = comparison;
                }
            }

            return result;
        }

        public static string DeQuote(this string me)
        {
            return me.Replace("\"", "");
        }

        public static bool ContainsSpecialCharacters(this string s)
        {
            Regex RgxUrl = new Regex("[^a-zA-Z0-9]");
            return RgxUrl.IsMatch(s);
        }
    }
}
