using FilterPolishUtil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FilterCore.FilterComponents.Tags
{
    public class TierTag
    {
        // Type->Currency
        // Type->Currency->Harbinger

        public List<string> Tags { get; set; } = new List<string>();
        public string PrimaryTag => Tags.FirstOrDefault();

        public void AppendToLastPart(string append)
        {
            this.Tags[this.Tags.Count - 1] += append;
        }

        public string CombinedTagValue =>
            StringWork.CombinePieces("->", Tags.Skip(1).ToList());

        public TierTag(params string[] tags)
        {
            this.Tags = new List<string>();
            this.Tags.AddRange(tags);
        }

        public bool IsPartOf(TierTag comparison)
        {
            return this.IsPartOf(comparison.Tags);
        }

        public bool IsPartOf(List<string> comparison)
        {
            if (comparison.Count < this.Tags.Count)
            {
                return false;
            }

            int i = 0;
            foreach (var item in Tags)
            {
                if (!item.Equals(comparison[i], System.StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
                i++;
            }

            return true;
        }

        public string Serialize()
        {
            return $"${StringWork.CombinePieces("->", this.Tags)}";
        }

        public TierTag Clone()
        {
            
            return new TierTag() {  Tags = this.Tags.Select(x => x).ToList() };
        }
    }
}
