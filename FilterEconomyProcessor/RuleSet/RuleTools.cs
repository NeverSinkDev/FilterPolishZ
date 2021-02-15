using FilterEconomy.Model;
using FilterPolishUtil.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FilterEconomyProcessor.RuleSet
{
    public static class RuleTools
    {
        public static bool HasAspect(this ItemList<NinjaItem> me, string s)
        {
            return me.Any(z => z.Aspects.Where(x => x.IsActive()).Any(j => j.Name == s));
        }

        public static List<NinjaItem> OfAspect(this ItemList<NinjaItem> me, string s)
        {
            return me.Where(z => z.Aspects.Where(x => x.IsActive()).Any(j => j.Name == s && j.IsActive())).ToList();
        }

        public static List<NinjaItem> AspectCheck(this ItemList<NinjaItem> me, HashSet<string> include, HashSet<string> exclude)
        {
            return me.Where(
                z => z.Aspects.Where(x => x.IsActive()).Any(x => include.Contains(x.Name) || include.Count == 0) &&
                     z.Aspects.Where(x => x.IsActive()).All(x => !exclude.Contains(x.Name))).ToList();
        }

        public static bool NoAspect(this ItemList<NinjaItem> me, string excludedAspect)
        {
            return me.All(x => x.Aspects.All(y => y.Name != excludedAspect));
        }

        public static bool AllUnhandled(this ItemList<NinjaItem> me)
        {
            return me.NoAspect("HandledAspect");
        }

        public static bool AllItemsFullFill(this List<NinjaItem> me, HashSet<string> include, HashSet<string> exclude)
        {
            return me.All(
                z => z.Aspects.Where(x => x.IsActive()).Any(x => include.Contains(x.Name) || include.Count == 0) &&
                     z.Aspects.Where(x => x.IsActive()).All(x => !exclude.Contains(x.Name)));
        }
    }
}
