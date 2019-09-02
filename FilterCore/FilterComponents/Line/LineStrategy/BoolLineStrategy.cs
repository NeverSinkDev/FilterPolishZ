using System.Collections.Generic;
using System.Linq;
using FilterCore.Line;

namespace FilterDomain.LineStrategy
{
    public class BoolLineStrategy : ILineStrategy
    {
        public bool CanHaveOperator => false;
        public bool CanHaveComment => false;
        public bool CanHaveMultiple => false;

        public IFilterLine Construct(string ident, List<LineToken> tokens)
        {
            return this.Construct<BoolValueContainer>(ident, tokens);
        }
    }

    public class BoolValueContainer : ILineValueCore
    {
        public bool Value;

        public void Parse(List<LineToken> tokens)
        {
            if (tokens.First().value == "True")
            {
                Value = true;
            }
            else if (tokens.First().value == "False")
            {
                Value = false;
            }
            else
            {
                throw new System.Exception("Unkown BOOL Token Value: " + tokens.First().value);
            }
        }

        public ILineValueCore Clone()
        {
            return new BoolValueContainer
            {
                Value = this.Value
            };
        }

        public string Serialize()
        {
            return Value ? "True" : "False";
        }
    }
}
