using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterEconomy.Model
{
    public class Suggestion
    {
        // 1 => moved up
        // 0 => stayed same
        // -1 => lost tiers
        public int Change { get; set; } = 0;
        public string BaseType { get; set; }
        public string SubGroup { get; set; }
        public string InnerInformation { get; set; }
        public string OldTier { get; set; }
        public string NewTier { get; set; }
        public string Reason { get; set; }
    }
}
