using FilterCore.Constants;
using FilterCore.Line;
using FilterDomain.LineStrategy;
using System.Collections.Generic;
using System.Linq;

namespace FilterCore.Entry
{
    public static class EFilterEntry
    {
        public static IEnumerable<FilterLine<T>> GetLines<T>(this IFilterEntry me, string ident) where T : ILineValueCore
        {
            if (me.Header.Type != FilterGenerationConfig.FilterEntryType.Content)
            {
                yield break;
            }

            var results = me.Content.Content.Where(x => x.Key == ident).SelectMany(x => x.Value).ToList();

            foreach (var item in results)
            {
                yield return item as FilterLine<T>;
            }
        }

        public static IEnumerable<T> GetValues<T>(this IFilterEntry me, string ident) where T : class, ILineValueCore
        {
            var results = me.GetLines<T>(ident);

            foreach (var item in results)
            {
                yield return item.Value as T;
            }
        }
    }
}
