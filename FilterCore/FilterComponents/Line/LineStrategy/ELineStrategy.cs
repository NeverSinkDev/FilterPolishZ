using FilterCore.Line;
using System.Collections.Generic;

namespace FilterDomain.LineStrategy
{
    public static class ELineStrategy
    {
        public static IFilterLine Construct<T>(this ILineStrategy me, string ident, List<LineToken> tokens) where T : ILineValueCore, new()
        {
            var line = new FilterLine<T>()
            {
                Ident = ident,
                Value = new T()
            };

            line.Value.Parse(tokens);
            return line;
        }
    }
}
