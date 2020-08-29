using FilterPolishUtil;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public class ExoAtom
    {
        public string Value;
        public ExoAtomType IdentifiedType;
        public bool CanBeVariable = true;
        public bool IsOperator = false;

        public ExoAtom(string value)
        {
            Value = value;
            IsPossibleVariable();
        }

        public string ResolveVariable(ExoBlock parent)
        {
            if (this.CanBeVariable)
            {
                if (parent.IsVariable(this.Value))
                {
                    return GetVariableValue();
                }
            }

            string GetVariableValue() => string.Join(" ", parent.GetVariable(this.Value).GetValue());
            return this.Value;
        }

        public ExoFunction GetFunction(ExoBlock parent)
        {
            return parent.GetFunction(this.Value);
        }

        public void IsPossibleVariable()
        {

            if (this.Value.Length == 1 && FilterExoConfig.SimpleOperators.Contains(this.Value[0]))
            {
                if (this.Value == ",")
                {
                    IdentifiedType = ExoAtomType.util;
                }
                else
                {
                    IdentifiedType = ExoAtomType.oper;
                }


                IsOperator = true;
                CanBeVariable = false;
                return;
            }

            if (this.Value.All(x => char.IsDigit(x)))
            {
                IdentifiedType = ExoAtomType.prim;
                CanBeVariable = false;
                return;
            }

            if (this.Value.ContainsSpecialCharacters())
            {
                if (this.Value.First() == '"' && this.Value.Last() == '"')
                {
                    IdentifiedType = ExoAtomType.dict;
                }
                else
                {
                    IdentifiedType = ExoAtomType.prim;
                }

                CanBeVariable = false;
                return;
            }

            if (FilterCore.FilterGenerationConfig.TierTagSort.ContainsKey(this.Value))
            {
                IdentifiedType = ExoAtomType.prim;
                CanBeVariable = false;
                return;
            }

            if (FilterCore.FilterGenerationConfig.LineTypesSort.ContainsKey(this.Value))
            {
                IdentifiedType = ExoAtomType.prim;
                CanBeVariable = false;
                return;
            }

            if (FilterCore.FilterGenerationConfig.ValidRarities.Contains(this.Value))
            {
                IdentifiedType = ExoAtomType.prim;
                CanBeVariable = false;
                return;
            }
        }
    }
}
