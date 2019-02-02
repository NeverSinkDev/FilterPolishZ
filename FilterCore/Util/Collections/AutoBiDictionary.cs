using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterCore.Util.Collections
{
    public class AutoBiDictionary<K, T>
    {
        private Dictionary<K, T> storageLeft = new Dictionary<K, T>();
        private Dictionary<T, K> storageRight = new Dictionary<T, K>();



        public T this[K key]
        {
            get
            {
                if (!this.storageLeft.ContainsKey(key))
                {
                    throw new Exception("Key not found");
                }

                return this.storageLeft[key];
            }

            set
            {
                if (value == null)
                {
                    var tempValue = this.storageLeft[key];
                    this.storageRight.Remove(tempValue);
                    this.storageLeft.Remove(key);
                    return;
                }

                if (!this.storageLeft.ContainsKey(key))
                {
                    this.storageRight.Add(value, key);
                    this.storageLeft.Add(key, value);
                    return;
                }

                this.storageLeft[key] = value;
                this.storageRight[value] = key;
            }
        }

        public K this[T key]
        {
            get
            {
                if (!this.storageRight.ContainsKey(key))
                {
                    throw new Exception("Key not found");
                }

                return this.storageRight[key];
            }

            set
            {
                if (value == null)
                {
                    var tempValue = this.storageRight[key];
                    this.storageRight.Remove(key);
                    this.storageLeft.Remove(tempValue);
                    return;
                }

                if (!this.storageRight.ContainsKey(key))
                {
                    this.storageRight.Add(key, value);
                    this.storageLeft.Add(value, key);
                    return;
                }

                this.storageRight[key] = value;
                this.storageLeft[value] = key;
            }
        }

        public bool ContainsKey(K id)
        {
            if (this.storageLeft.ContainsKey(id))
            {
                return true;
            }
            return false;
        }

        public bool ContainsKey(T id)
        {
            if (this.storageRight.ContainsKey(id))
            {
                return true;
            }
            return false;
        }
    }
}
