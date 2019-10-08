using FilterDomain.LineStrategy;
using System.Collections.Generic;
using System.Linq;

namespace FilterCore.FilterComponents.Tier
{
    public class TierGroup
    {
        public TierGroup(string category)
        {
            this.Category = category;

            if (this.Category.ToLower().Contains("prophecy"))
            {
                this.KeyIdent = "Prophecy";
            }
        }

        public string KeyIdent { get; set; } = "BaseType";
        public string Category { get; set; }

        // TierKey : Tier
        public Dictionary<string, SingleTier> FilterEntries { get; set; } = new Dictionary<string, SingleTier>();

        // List of entries that do not fit in the "disable if empty" concept
        public HashSet<string> NonBaseTypeEntries { get; set; } = new HashSet<string>();

        // Item : TierKey
        public Dictionary<string, List<string>> ItemTiering = new Dictionary<string, List<string>>();

        public void ReEvaluate()
        {
            ItemTiering.Clear();
            foreach (var category in this.FilterEntries)
            {
                var containers = category.Value.Entry.Select(x => x.Content.GetFirst(this.KeyIdent)?.Value as EnumValueContainer).Where(z => z!= null).ToList();
                var linetokens = containers.SelectMany(x => x?.Value.ToList()).Where(z => z!= null).ToList();
                var results = linetokens.Select(x => x.value).ToList();

                foreach (var item in results)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    if (!ItemTiering.ContainsKey(item))
                    {
                        ItemTiering.Add(item, new List<string>());
                    }

                    ItemTiering[item].Add(category.Key);
                }
            }
        }
    }
}

