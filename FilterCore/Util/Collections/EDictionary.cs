using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterCore.Util.Collections
{
    public static class EDictionary
    {
        public static void AddOrSet<TKey,TValue>(this Dictionary<TKey,TValue> me, TKey key, TValue value)
        {
            if (!me.ContainsKey(key))
            {
                me.Add(key, value);
            }
            else
            {
                me[key] = value;
            }
        }

        public static void AddIfNew<TKey, TValue>(this Dictionary<TKey, TValue> me, TKey key, TValue value)
        {
            if (!me.ContainsKey(key))
            {
                me.Add(key, value);
            }
        }
    }
}
