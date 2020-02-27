using System;
using System.Collections.Generic;
using System.Text;

namespace FilterPolishUtil.Collections
{
    public class NameResolvingStorage<T>
    {
        public NameResolvingStorage()
        {

        }

        public NameResolvingStorage(T defaultValue)
        {
            this.defaultValue = defaultValue;
            this.useDefaultValue = true;
        }

        private Dictionary<short, T> storage = new Dictionary<short, T>();
        private AutoDictionary<string, short> nameProvider = new AutoDictionary<string, short>();
        private short KeyCounter = 1;
        private Queue<short> AvailableKeys { get; set; } = new Queue<short>();

        private bool useDefaultValue = false;
        private T defaultValue;


        public T this[short key]
        {
            get
            {
                if (!this.storage.ContainsKey(key))
                {
                    if (this.useDefaultValue)
                    {
                        this.Register(this.defaultValue);
                    }
                    else
                    {
                        throw new Exception("Key not found");
                    }
                }

                return this.storage[key];
            }

            set
            {
                if (value == null)
                {
                    this.storage.Remove(key);
                    this.AvailableKeys.Enqueue(key);
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

        public short Register(T obj)
        {
            short key;
            if (this.AvailableKeys.Count > 0)
            {
                key = this.AvailableKeys.Dequeue();

            }
            else
            {
                key = KeyCounter++;
            }

            this[key] = obj;
            return key;
        }

        public short RegisterAndName(T obj, string name)
        {
            short num = this.Register(obj);
            this.AssignNameToNum(name, num);
            return num;
        }

        public void Clear()
        {
            this.AvailableKeys.Clear();
            this.KeyCounter = 1;
            this.nameProvider.Clear();
            this.storage.Clear();
        }

        public void AssignNameToNum(string str, short num)
        {
            this.nameProvider[str] = num;
        }

        public T GetValueByName(string str)
        {
            return this[this.nameProvider[str]];
        }

        public short GetKeyByName(string str)
        {
            return this.nameProvider[str];
        }

        public bool ContainsValue(string str)
        {
            return this.nameProvider.ContainsKey(str) && this.ContainsKey(this.nameProvider[str]);
        }

        public IEnumerable<T> YieldValues()
        {
            foreach (var i in this.storage.Values)
            {
                yield return i;
            }
        }

        public bool ContainsKey(short id)
        {
            if (this.storage.ContainsKey(id))
            {
                return true;
            }
            return false;
        }
    }
}
