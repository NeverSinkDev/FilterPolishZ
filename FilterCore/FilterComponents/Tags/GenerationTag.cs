using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterCore.FilterComponents.Tags
{
    public class GenerationTag
    {
        public string Value { get; set; }
        public short Strictness { get; set; } = -1;

        public string Serialize()
        {
            if (Strictness >= 0)
            {
                return $"%{Value}{Strictness}";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
