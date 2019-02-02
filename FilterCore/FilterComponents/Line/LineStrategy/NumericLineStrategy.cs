using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilterCore.Constants;
using FilterCore.Line;

namespace FilterDomain.LineStrategy
{
    public class NumericLineStrategy : ILineStrategy
    {
        public bool CanHaveOperator => true;
        public bool CanHaveComment => false;
        public bool CanHaveMultiple => true;

        public IFilterLine Construct(string ident, List<LineToken> tokens)
        {
            return this.Construct<NumericValueContainer>(ident, tokens);
        }
    }

    public class NumericValueContainer : ILineValueCore
    {
        public string Value;
        public string Operator;

        public void Parse(List<LineToken> tokens)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                if (i == 0 && FilterConstants.FilterOperators.Contains(tokens[0].value))
                {
                    Operator = tokens[0].value;
                }
                else if (i == 0)
                {
                    Operator = null;
                    Value = tokens[0].value;
                }

                if (i == 1 && Operator != null)
                {
                    Value = tokens[1].value;
                }
            }
        }

        public string Serialize()
        {
            if (Operator == "=")
            {
                return this.Value;
            }
            else
            {
                return $"{Operator?.ToString() ?? string.Empty} {Value.ToString()}";
            }
        }
    }
}
