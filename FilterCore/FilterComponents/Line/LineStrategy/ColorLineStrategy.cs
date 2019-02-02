using FilterCore.Line;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterDomain.LineStrategy
{
    public class ColorLineStrategy : ILineStrategy
    {
        public bool CanHaveOperator => false;
        public bool CanHaveComment => true;
        public bool CanHaveMultiple => false;

        public IFilterLine Construct(string ident, List<LineToken> tokens)
        {
            return this.Construct<ColorValueContainer>(ident, tokens);
        }
    }

    public class ColorValueContainer : ILineValueCore
    {
        public short R = 0;
        public short G = 0;
        public short B = 0;
        public short O = -1;

        public void Parse(List<LineToken> tokens)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                if (i == 0)
                {
                    R = short.Parse(tokens[0].value);
                }

                if (i == 1)
                {
                    G = short.Parse(tokens[1].value);
                }

                if (i == 2)
                {
                    B = short.Parse(tokens[2].value);
                }

                if (i == 3)
                {
                    O = short.Parse(tokens[3].value);
                }
            }
        }

        public string Serialize()
        {
            if (O == -1)
            {
                return $"{R.ToString()} {G.ToString()} {B.ToString()}";
            }
            else
            {
                return $"{R.ToString()} {G.ToString()} {B.ToString()} {O.ToString()}";
            }
        }
    }
}
