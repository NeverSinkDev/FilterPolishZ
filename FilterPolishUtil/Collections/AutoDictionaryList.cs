using System;
using System.Collections.Generic;
using System.Text;

namespace FilterPolishUtil.Collections
{
    public class AutoDictionaryList<K,V>
    {
        public Dictionary<K, List<V>> dict = new Dictionary<K, List<V>>();

        public V this[K key, int number] => dict[key][number];

        public IEnumerable<V> this[K key]
        {
            get
            {
                foreach (var value in this.dict[key])
                {
                    yield return value;
                }
            }
        }

        public void Add(K key, V value)
        {
            if (!this.dict.ContainsKey(key))
            {
                this.dict.Add(key, new List<V>());
            }

            this.dict[key].Add(value);
        }
    }
}
