using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool CanHaveOperator => false;
        public bool CanHaveComment => false;
        public bool CanHaveMultiple => false;
    }
}
