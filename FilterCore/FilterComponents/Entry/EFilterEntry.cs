using FilterCore.Constants;
using FilterCore.Line;
using FilterDomain.LineStrategy;
using System.Collections.Generic;
using System.Linq;

namespace FilterCore.Entry
{
    public static class EFilterEntry
    {
        public static IEnumerable<FilterLine<T>> GetLines<T>(this IFilterEntry me, string ident) where T : class, ILineValueCore
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

        public static IEnumerable<IFilterLine> GetLines(this IFilterEntry me, string ident)
        {
            if (me.Header.Type != FilterGenerationConfig.FilterEntryType.Content)
            {
                yield break;
            }

            var results = me.Content.Content.Where(x => x.Key == ident).SelectMany(x => x.Value).ToList();

            foreach (var item in results)
            {
                yield return item;
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

        public static bool HasLine<T>(this IFilterEntry me, string ident)
        {
            var list = me.GetLines(ident).ToList();

            if (list == null || list.Count == 0 )
            {
                return false;
            }

            return true;
        }

        public static bool EnableIfValid(this IFilterEntry me, string ident)
        {
            var valid = me.GetLines(ident)?.All(x => x.Value.IsValid()) ?? true;

            if (valid)
            {
                me.SetEnabled(true);
            }

            return true;
        }

        public static bool DisableIfInvalid(this IFilterEntry me, string ident)
        {
            var valid = me.GetLines(ident)?.All(x => x.Value.IsValid()) ?? false;

            if (!valid)
            {
                me.SetEnabled(false);
            }

            return true;
        }

        public static void SetEnabled(this IFilterEntry me, bool enabled)
        {
            if (me.Header.Type != FilterGenerationConfig.FilterEntryType.Content)
            {
                return;
            }

            me.Header.IsFrozen = !enabled;
        }
    }
}
