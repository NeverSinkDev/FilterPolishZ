using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishUtil.Extensions
{
    public static class KeyValuePairExtensions
    {
        public static bool IsNull<T, TU>(this KeyValuePair<T, TU> pair)
        {
            return pair.Equals(new KeyValuePair<T, TU>());
        }
    }

    public static class EIEnumerable
    {
        public static List<T> RemoveFrom<T>(this List<T> lst, int from)
        {
            if (lst.Count <= from)
            {
                return lst;
            }

            lst.RemoveRange(lst.Count - from, from);
            return lst;
        }

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }

        public static void RemoveAll<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            T element;

            for (int i = 0; i < collection.Count; i++)
            {
                element = collection.ElementAt(i);
                if (predicate(element))
                {
                    collection.Remove(element);
                    i--;
                }
            }
        }

        public static (List<T> before, List<T> after) SplitBy<T>(this List<T> me, Predicate<T> search)
        {
            var index = me.FindIndex(search);
            if (index == -1)
            {
                return (null, null);
            }

            return (me.Take(index).ToList(), me.Skip(index + 1).ToList());
        }

        public static List<List<T>> SplitDivide<T>(this List<T> me, Predicate<T> splitter)
        {
            var result = new List<List<T>>();
            var current = new List<T>();
            foreach (var item in me)
            {
                if (splitter(item))
                {
                    result.Add(current);
                    current = new List<T>();
                }
                else
                {
                    current.Add(item);
                }
            }

            result.Add(current);

            return result;
        }

        public static bool ConfirmPattern<T>(this List<T> me, params Predicate<T>[] pattern)
        {
            if (me.Count != pattern.Length)
            {
                return false;
            }

            for (int i = 0; i < pattern.Length; i++)
            {
                if(!pattern[i](me[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<T> YieldTogether<T>(params IEnumerable<T>[] collections)
        {

            for (int i = 0; i < collections.Length; i++)
            {
                foreach (var item in collections[i])
                {
                    yield return item;
                }
            }
        }

        public static T Not<T,T1>(this T collection, T1 except) where T : ICollection<T1>, new() where T1 : IComparable
        {
            T result = new T();
            foreach (T1 item in collection)
            {
                if (!item.Equals(except))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public static T Not<T, T1>(this T collection, Func<T1,bool> except) where T : ICollection<T1>, new() where T1 : IComparable
        {
            T result = new T();
            foreach (T1 item in collection)
            {
                if (!except(item))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public static void ForEachIndexing<T>(this IList<T> collection, Func<T, int, T> func)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                collection[i] = func(collection[i],i);
            }
        }

        public static void ForEachIndexing<T>(this IList<T> collection, Action<T, int> action)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                action(collection[i], i);
            }
        }

        public static IEnumerable<T> MaxBy<T>(this IEnumerable<T> me, Func<T, IComparable> comparison, float percentage = default(float))
        {
            var analyzedList = me.Select(x => new { val = x, com = comparison(x) });
            IComparable max = analyzedList.Max(x => x.com);
            return analyzedList.Where(x => IComparable.Equals(x.com,max)).Select(x => x.val);
        }

        public static Dictionary<string, List<T>> Subdivide<T>(this IEnumerable<T> me, List<Tuple<string, Func<T, bool>>> rules, bool hasDefault = false)
        {
            var result = new Dictionary<string, List<T>>();

            foreach (var rule in rules)
            {
                if (!result.ContainsKey(rule.Item1))
                {
                    result.Add(rule.Item1, new List<T>());
                }
            }

            if (hasDefault)
            {
                result.Add("default", new List<T>());
            }

            foreach (var item in me)
            {
                var success = false;
                foreach (var rule in rules)
                {
                    if (rule.Item2(item))
                    {
                        result[rule.Item1].Add(item);
                        success = true;
                        break;
                    }
                }

                if (hasDefault && !success)
                {
                    result["default"].Add(item);
                }
            }

            return result;
        }

        public static IEnumerable<T1> PairSelect<T,T1>(this IEnumerable<T> collection, Func<T,T,T1> selector)
        {
            var initialized = false;
            T current = default(T);
            T previous = default(T);

            foreach (var item in collection)
            {
                previous = current;
                current = item;

                if (!initialized)
                {
                    initialized = true;
                    continue;
                }

                yield return selector(previous, current);
            }
        }

        public static List<T> SelectInnerContents<T>(this IEnumerable<T> collection, Predicate<T> first, Predicate<T> second)
        {
            int state = 0;
            List<T> results = new List<T>();

            foreach (var item in collection)
            {
                if (state == 0)
                {
                    if (first(item))
                    {
                        state++;
                    }
                }
                else if (state == 1)
                {
                    if (second(item))
                    {
                        state++;
                    }
                    else
                    {
                        results.Add(item);
                    }
                }

                if (state == 2)
                {
                    return results;
                }
            }

            return null;
        }
    }
}
