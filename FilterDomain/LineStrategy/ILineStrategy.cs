using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterDomain.LineStrategy
{
    public interface ILineStrategy
    {
        bool CanHaveOperator { get; }
        bool CanHaveComment { get; }
        bool CanHaveMultiple { get; }
    }
}
