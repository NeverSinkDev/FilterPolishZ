using FilterCore.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterCore.FilterComponents.Tags
{
    public class TierTag
    {
        public List<string> Tags { get; set; } = new List<string>();
        public string PrimaryTag => Tags.FirstOrDefault();

        public TierTag(params string[] tags)
        {
            this.Tags = new List<string>();
            this.Tags.AddRange(tags);
        }

        public string Serialize()
        {
            return $"${StringWork.CombinePieces("->", this.Tags)}";
        }
    }
}
