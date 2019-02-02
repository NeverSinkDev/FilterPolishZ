using FilterCore.Line;
using FilterDomain.LineStrategy;
using System.Collections.Generic;
using System.Linq;

namespace FilterCore.Entry
{
    public static class EFilterEntry
    {
        public static FilterLine<T> GetLines<T>(this IFilterEntry me, string type) where T : ILineValueCore
        {
            return me.Content.GetFirst(type) as FilterLine<T>;
        }

        public static List<FilterLine<T>> GetLine<T>(this IFilterEntry me, string type) where T : ILineValueCore
        {
            return me.Content.Get(type) as List<FilterLine<T>>;
        }

        public static T GetLineValue<T>(this IFilterEntry me, string type) where T : class, ILineValueCore
        {
            return (me.Content.GetFirst(type).Value as T);
        }

        public static List<T> GetLineValues<T>(this IFilterEntry me, string type) where T : ILineValueCore
        {
            return (me.Content.Get(type).Select(x => x.Value).ToList() as List<T>);
        }
    }
}
