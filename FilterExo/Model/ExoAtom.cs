using FilterPolishUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FilterExo.Model
{
    public enum ExoAtomType
    {
        dict,   // converted into a dictionary type representation. Their combined order doesn't matter
        prim,   // not combinable, instead primitive values that have to keep their order
        util,   // "," usually
        oper    // + - 
    }

    [DebuggerDisplay("{debugView}")]
    public class ExoAtom
    {
        private string debugView => $"{this.GetRawValue()} ({IdentifiedType})";

        public IExoAtomValueCore ValueCore;
        public ExoAtomType IdentifiedType;

        public ExoAtom(HashSet<string> value)
        {
            if (!value.All(x => IsStringType(x)))
            {
                TraceUtility.Throw("Can't create merged ExoAtom from non-dictionary types!");
            }

            this.IdentifiedType = ExoAtomType.dict;
            this.ValueCore = new HashSetValueCore() { Values = value };
        }

        public ExoAtom(List<string> value)
        {
            if (!value.All(x => IsStringType(x)))
            {
                TraceUtility.Throw("Can't create merged ExoAtom from non-dictionary types!");
            }

            this.IdentifiedType = ExoAtomType.dict;
            this.ValueCore = new HashSetValueCore() { Values = new HashSet<string>(value) };
        }

        public ExoAtom(string value)
        {
            if (IsReservedType(value) || value.All(x => char.IsDigit(x)))
            {
                this.IdentifiedType = ExoAtomType.prim;
                this.ValueCore = new SimpleAtomValueCore() { Value = value, CanBeVariable = false };
            }
            else if (IsStringType(value))
            {
                this.IdentifiedType = ExoAtomType.dict;
                this.ValueCore = new HashSetValueCore() { Values = new HashSet<string>() { value } }; // ??
            }
            else if (value.Length <= 2 && FilterExoConfig.SimpleOperators.Contains(value[0]) || FilterExoConfig.CombinedOperators.Contains(value))
            {
                this.IdentifiedType = ExoAtomType.oper;
                this.ValueCore = new SimpleAtomValueCore() { Value = value, CanBeVariable = false };
            }
            else if (value.ContainsSpecialCharacters())
            {
                TraceUtility.Throw("Unknown ExoAtom Type with special characters: " + value);
            }
            else
            {
                this.IdentifiedType = ExoAtomType.prim;
                this.ValueCore = new SimpleAtomValueCore() { Value = value, CanBeVariable = true };
            }
        }

        public string Serialize(ExoBlock parent)
        {
            return this.ValueCore.Serialize(parent);
        }

        public string GetRawValue()
        {
            return this.ValueCore.GetRawValue();
        }

        public ExoFunction GetFunction(ExoBlock parent)
        {
            // TODO
            return parent.GetFunction(this.Serialize(parent));
        }

        public static bool IsStringType(string value)
        {
            if (value.First() == '"' && value.Last() == '"')
            {
                return true;
            }

            return false;
        }

        public static bool IsReservedType(string value)
        {
            if (FilterCore.FilterGenerationConfig.TierTagSort.ContainsKey(value))
            {
                return true;
            }

            if (FilterCore.FilterGenerationConfig.LineTypesSort.ContainsKey(value))
            {
                return true;
            }

            if (FilterCore.FilterGenerationConfig.ValidRarities.Contains(value))
            {
                return true;
            }

            return false;
        }
    }
}
