using FilterCore.Line;
using FilterPolishUtil.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterDomain.LineStrategy
{
    public class EnumLineStrategy : ILineStrategy
    {
        public bool CanHaveOperator => false;
        public bool CanHaveComment => false;
        public bool CanHaveMultiple => true;

        public IFilterLine Construct(string ident, List<LineToken> tokens)
        {
            return this.Construct<EnumValueContainer>(ident, tokens);
        }
    }

    public class EnumValueContainer : ILineValueCore
    {
        public HashSet<LineToken> Value = new HashSet<LineToken>();

        public void Parse(List<LineToken> tokens)
        {
            foreach (var item in tokens)
            {
                if (!Value.Contains(item))
                {
                    Value.Add(item);
                }
            }
        }

        public ILineValueCore Clone()
        {
            return new EnumValueContainer
            {
                Value = new HashSet<LineToken>(this.Value.Select(x => x.Clone()))
            };
        }

        public string Serialize()
        {
            if (this.Value.Count == 0)
            {
                LoggingFacade.LogError("ERROR! Empty line in EnumValueContainer!");
            }

            return string.Join(" ", this.Value.ToList().OrderBy(x => x.value).Select(z => z.Serialize()).Distinct().ToList());
        }
    }
}
