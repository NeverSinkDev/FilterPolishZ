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
        public EnumLineStrategy(bool sortValues)
        {
            this.SortValues = sortValues;
        }

        public bool SortValues = true;
        public bool CanHaveOperator => false;
        public bool CanHaveComment => false;
        public bool CanHaveMultiple => true;

        public IFilterLine Construct(string ident, List<LineToken> tokens)
        {
            var line = this.Construct<EnumValueContainer>(ident, tokens);
            (line.Value as EnumValueContainer).PerformSort = SortValues;
            return line;
        }
    }

    public class EnumValueContainer : ILineValueCore
    {
        public bool PerformSort = true;
        public HashSet<LineToken> Value = new HashSet<LineToken>();
        public bool ExactSearch = false;

        public void Parse(List<LineToken> tokens)
        {
            if (tokens[0].value == "==")
            {
                ExactSearch = true;
                tokens.RemoveAt(0);
            }

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
            List<string> value;

            // skip sorting for ExplicitModFiltering
            if (this.PerformSort)
            {
                value = this.Value.ToList().OrderBy(x => x.value).Select(z => z.Serialize()).Distinct().ToList();
            }
            else
            {
                value = this.Value.ToList().Select(z => z.Serialize()).Distinct().ToList();
            }

            // add exactsearch operator
            if (this.ExactSearch)
            {
                value.Insert(0, "==");
            }

            return string.Join(" ", value);
        }

        public bool IsValid()
        {
            if (this.Value == null || this.Value.Count == 0)
            {
                return false;
            }

            return true;
        }
    }
}
