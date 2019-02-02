using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterDomain.LineStrategy
{
    public class NumericLineStrategy : ILineStrategy
    {
        public bool CanHaveOperator => true;
        public bool CanHaveComment => false;
        public bool CanHaveMultiple => true;
    }
}
