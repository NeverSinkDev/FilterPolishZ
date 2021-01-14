using FilterPolishUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FilterPolishUtil.Collections;

namespace FilterExo.Model
{
    public interface IExoAtomValueCore
    {
        string Serialize(ExoBlock parent);
        string GetRawValue();
        IEnumerable<ExoAtom> Resolve(ExoBlock parent);
    }

    public class HashSetValueCore : IExoAtomValueCore
    {
        public OrderedSet<string> Values;

        public string GetRawValue()
        {
            return string.Empty;
        }

        public IEnumerable<ExoAtom> Resolve(ExoBlock parent)
        {
            yield break;
        }

        public string Serialize(ExoBlock parent)
        {
            return string.Join(" ",this.Values.ToList());
        }
    }

    public class PackedValueCore : IExoAtomValueCore
    {
        public List<ExoAtom> Values;

        public IEnumerable<ExoAtom> Resolve(ExoBlock parent)
        {
            foreach (var item in Values)
            {
                yield return item;
            }
        }

        public string GetRawValue()
        {
            return string.Join(" ", Values.Select(x => x.GetRawValue()));
        }

        public string Serialize(ExoBlock parent)
        {
            return string.Join(" ", this.Values.Select(x => x.Serialize(parent)));
        }
    }

    /// <summary>
    /// Either primitive value or variable refering to a value
    /// </summary>
    public class SingularValueCore : IExoAtomValueCore
    {
        public string Value;
        public bool CanBeVariable;

        public string GetRawValue()
        {
            if (this.CanBeVariable)
            {
                return this.Value;
            }

            return this.Value;
        }

        public string Serialize(ExoBlock parent)
        {
            TraceUtility.Check(this.CanBeVariable && parent == null, "Attempting to serialize a potential variable without a parent!");

            if (CanBeVariable && parent.IsVariable(Value))
            {
                return parent.GetVariable(this.Value).Serialize(parent);
            }

            return this.Value;
        }

        public IEnumerable<ExoAtom> Resolve(ExoBlock parent)
        {
            if (CanBeVariable)
            {
                if (parent.IsVariable(Value))
                {
                    yield return parent.GetVariable(this.Value);
                }
                else if (parent.IsFunction(Value))
                {
                    yield return parent.GetFunction(this.Value);
                }
            }
        }

        public class FuncValueCore : IExoAtomValueCore
        {
            public ExoFunction Value;

            public string GetRawValue()
            {
                return Value.Name;
            }

            public IEnumerable<ExoAtom> Resolve(ExoBlock parent)
            {
                yield break;
            }

            public string Serialize(ExoBlock parent)
            {
                return Value.Name;
            }
        }
    }
}
