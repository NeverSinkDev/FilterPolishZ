﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FilterPolishUtil
{
    public static class StringWork
    {
        // If you want to implement both "*" and "?"

        public static string WildCardToRegular(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        public static string SubStringLast(this string me, string substring)
        {
            if (string.IsNullOrEmpty(me) || !me.Contains(substring))
            {
                return me;
            }

            var index = me.LastIndexOf(substring)+substring.Length;
            return me.Substring(index);
        }

        public static string SkipSegment(this string me, string segment)
        {
            return me.Substring(me.LastIndexOf(segment) + segment.Length);
        }

        public static string CombinePieces(string combiner, List<string> pieces)
        {
            return CombinePieces(combiner, pieces.ToArray());
        }

        public static string CombinePieces(string combiner, params string[] pieces)
        {
            combiner = string.IsNullOrEmpty(combiner) ? " " : combiner;
            var first = true;
            StringBuilder builder = new StringBuilder();

            foreach (var piece in pieces)
            {
                if (!string.IsNullOrEmpty(piece))
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        builder.Append(combiner);
                    }

                    builder.Append(piece);
                }
            }

            return builder.ToString();
        }

        public static string GetDateString()
        {
            return DateTime.Now.ToString("yyyy-M-d");
        }
    }
}
