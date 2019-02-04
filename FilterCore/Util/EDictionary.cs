using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterCore.Util
{
    public static class EDictionary
    {
        public static void AddOrCreate<T,K>(this IDictionary<T,K> me, T key, K value)
        {
            if (!me.ContainsKey(key))
            {
                me.Add(key, value);
            }

            me[key] = value;
        }
    }
}
