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

        public static bool ContainsSpecialCharacters(this string s)
        {
            Regex RgxUrl = new Regex("[^a-zA-Z0-9]");
            return RgxUrl.IsMatch(s);
        }
    }
}
