using FilterCore.Entry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterCore.FilterComponents.Tier
{
    public class SingleTier
    {
        public string TierName { get; set; }
        public List<IFilterEntry> Entry { get; set; } = new List<IFilterEntry>();
    }
}
