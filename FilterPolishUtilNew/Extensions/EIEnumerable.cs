using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishUtil.Extensions
{
    public static class EIEnumerable
    {
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
    }
}
