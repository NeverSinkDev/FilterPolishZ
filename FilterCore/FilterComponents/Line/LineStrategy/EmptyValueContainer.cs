using FilterDomain.LineStrategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterCore.Line.LineStrategy
{
    public class EmptyValueContainer : ILineValueCore
    {
        public void Parse(List<LineToken> tokens)
        {
            
        }

        public ILineValueCore Clone()
        {
            return new EmptyValueContainer();
        }

        public string Serialize()
        {
            return string.Empty;
        }

        public bool IsValid()
        {
            return true;
        }
    }
}
