using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishUtil
{
    public static class StringWork
    {
        public static string SubStringLast(this string me, string substring)
        {
            if (string.IsNullOrEmpty(me) || !me.Contains(substring))
            {
                return me;
            }

            var index = me.LastIndexOf(substring);
            return me.Substring(index + 1);
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
