using FilterCore.Line;
using FilterPolishUtil;
using System.Collections.Generic;
using System.Linq;

namespace FilterDomain.LineStrategy
{
    public class VariableLineStrategy : ILineStrategy
    {
        private short minCount;
        private short maxCount;

        public VariableLineStrategy(short minCount, short maxCount)
        {
            this.minCount = minCount;
            this.maxCount = maxCount;
        }

        public IFilterLine Construct(string ident, List<LineToken> tokens)
        {
            return this.Construct<VariableValueContainer>(ident, tokens);
        }

        public bool CanHaveOperator => false;
        public bool CanHaveComment => false;
        public bool CanHaveMultiple => false;
    }

    public class VariableValueContainer : ILineValueCore
    {
        public List<LineToken> Value;

        public void Parse(List<LineToken> tokens)
        {
            Value = new List<LineToken>();
            for (int i = 0; i < tokens.Count; i++)
            {
                Value.Add(tokens[i]);
            }
        }

        public string Serialize()
        {
            return StringWork.CombinePieces(" ",this.Value.Select(x => x.Serialize()).ToArray());
        }
    }
}
