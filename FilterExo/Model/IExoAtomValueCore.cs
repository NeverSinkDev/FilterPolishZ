using FilterPolishUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilterExo.Model
{
    public interface IExoAtomValueCore
    {
        string Serialize(ExoBlock parent);
        string GetRawValue();
        ExoAtom Resolve(ExoBlock parent);
    }

    public class HashSetValueCore : IExoAtomValueCore
    {
        public HashSet<string> Values;

        public string GetRawValue()
        {
            return string.Empty;
        }

        public ExoAtom Resolve(ExoBlock parent)
        {
            return null;
        }

        public string Serialize(ExoBlock parent)
        {
            return string.Join(" ",this.Values.ToList());
        }
    }

    public class WildValueValueCore : IExoAtomValueCore
    {
        public List<ExoAtom> Values;

        public ExoAtom Resolve(ExoBlock parent)
        {
            return null;
        }

        public string GetRawValue()
        {
            return string.Empty;
        }

        public string Serialize(ExoBlock parent)
        {
            return string.Join(" ", this.Values.Select(x => x.Serialize(parent)));
        }
    }

    /// <summary>
    /// Either primitive value or variable refering to a value
    /// </summary>
    public class SimpleAtomValueCore : IExoAtomValueCore
    {
        public string Value;
        public bool CanBeVariable;

        public string GetRawValue()
        {
            if (this.CanBeVariable)
            {
                return string.Empty;
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

        public ExoAtom Resolve(ExoBlock parent)
        {
            if (CanBeVariable && parent.IsVariable(Value))
            {
                return parent.GetVariable(this.Value);
            }

            return null;
        }
    }
}
