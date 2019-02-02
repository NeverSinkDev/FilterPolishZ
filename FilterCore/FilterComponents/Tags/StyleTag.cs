using FilterCore.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterCore.FilterComponents.Tags
{
    public class StyleTag
    {
        public Path StylePath { get; set; }

        public string Serialize()
        {
            if (StylePath != null)
            {
                return $"${StylePath.Serialize()}";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
