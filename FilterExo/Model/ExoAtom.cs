using FilterPolishUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FilterExo.Model
{
    public class ExoAtom
    {
        public string Value;
        public bool CanBeVariable = true;
        public bool IsVariable;

        public ExoAtom(string value)
        {
            Value = value;
            IsPossibleVariable();
        }

        public string ResolveValue(ExoBlock parent)
        {
            if (this.IsVariable)
            {
                return GetVariableValue();
            }

            if (this.CanBeVariable)
            {
                if (parent.IsVariable(this.Value))
                {
                    this.IsVariable = true;
                    return GetVariableValue();
                }
            }

            string GetVariableValue() => string.Join(" ", parent.GetVariable(this.Value).GetValue());
            return this.Value;
        }

        public void IsPossibleVariable()
        {

            if (this.Value.All(x => char.IsDigit(x)))
            {
                CanBeVariable = false;
                IsVariable = false;
                return;
            }

            if (this.Value.ContainsSpecialCharacters())
            {
                CanBeVariable = false;
                IsVariable = false;
                return;
            }

            if (FilterCore.FilterGenerationConfig.TierTagSort.ContainsKey(this.Value))
            {
                CanBeVariable = false;
                IsVariable = false;
                return;
            }

            if (FilterCore.FilterGenerationConfig.LineTypesSort.ContainsKey(this.Value))
            {
                CanBeVariable = false;
                IsVariable = false;
                return;
            }

            if (FilterCore.FilterGenerationConfig.ValidRarities.Contains(this.Value))
            {
                CanBeVariable = false;
                IsVariable = false;
                return;
            }
        }
    }
}
