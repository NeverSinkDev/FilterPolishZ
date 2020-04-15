using FilterCore.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterCore.FilterComponents.Tags
{
    public class TierTagSet
    {
        public Dictionary<string, TierTag> TierTags { get; set; } = new Dictionary<string, TierTag>();

        public void Add(string s)
        {
            if (s[0] == '$')
            {
                s = s.Substring(1);
            }

            var separator = new string[] { "->" };
            if (s.Contains(separator[0]))
            {
                var parts = s.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                this.TierTags.Add(parts.First(), new TierTag(parts));
            }
            else
            {
                this.TierTags.Add(s, new TierTag(s));
            }
        }

        public bool IsPartOf(TierTag comparison)
        {
            if (!this.TierTags.ContainsKey(comparison.PrimaryTag))
            {
                return false;
            }

            return this.TierTags[comparison.PrimaryTag].IsPartOf(comparison);
        }

        public void AppendUpSuffixToTierTag()
        {
            if (this.TierTags.ContainsKey("tier"))
            {
                this.TierTags["tier"].AppendToLastPart("-up");
            }
        }

        public bool IsPartOf(List<string> comparison)
        {
            if (!this.TierTags.ContainsKey(comparison[0]))
            {
                return false;
            }

            return this.TierTags[comparison[0]].IsPartOf(comparison);
        }

        public bool ContainsKey(string s)
        {
            return this.TierTags.ContainsKey(s);
        }

        public TierTag this[string key]
        {
            get
            {
                return this.TierTags[key];
            }

            set { this.TierTags[key] = value; }
        }

        public string Serialize()
        {
            // todo -> sort
            return string.Join(" ",
                // .OrderBy(z => FilterConstants.TierTagSort[z])
                this.TierTags.Select(x => x.Value.Serialize()).ToList());
        }

        public TierTagSet Clone()
        {
            var newSet = new Dictionary<string, TierTag>();

            foreach (var item in this.TierTags)
            {
                newSet.Add(item.Key, item.Value.Clone());
            }

            return new TierTagSet
            {
                TierTags = newSet
            };
        }
    }
}
