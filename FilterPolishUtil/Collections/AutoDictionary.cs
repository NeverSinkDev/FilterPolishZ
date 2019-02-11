using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishUtil.Collections
{
    public class AutoDictionary<K, T>
    {
        /// <summary>
        /// While public the direct access should only be used as a means for easy comparison
        /// </summary>
        public Dictionary<K, T> storage = new Dictionary<K, T>();

        public T this[K key]
        {
            get
            {
                if (!this.storage.ContainsKey(key))
                {
                    throw new Exception("Key not found");
                }

                return this.storage[key];
            }

            set
            {
                if (value == null)
                {
                    this.storage.Remove(key);
                    return;
                }

                if (!this.storage.ContainsKey(key))
                {
                    this.storage.Add(key, value);
                    return;
                }

                this.storage[key] = value;
            }
        }

        public IEnumerable<T> YieldValues()
        {
            foreach (var i in this.storage.Values)
            {
                yield return i;
            }
        }

        public bool ContainsKey(K id)
        {
            if (this.storage.ContainsKey(id))
            {
                return true;
            }
            return false;
        }

        public int CalculateStashCount()
        {
            return this.storage.Keys.Count;
        }

        public bool HasItems()
        {
            return this.storage.Count != 0;
        }

        public void Clear()
        {
            this.storage.Clear();
        }
    }
}
