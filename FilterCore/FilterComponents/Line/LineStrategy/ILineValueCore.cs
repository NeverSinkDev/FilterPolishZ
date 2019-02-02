using FilterCore.Line;
using System.Collections.Generic;

namespace FilterDomain.LineStrategy
{
    public interface ILineValueCore
    {
        string Serialize();
        void Parse(List<LineToken> tokens);
    }
}
