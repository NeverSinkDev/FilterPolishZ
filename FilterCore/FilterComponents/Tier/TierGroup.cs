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
        }

        public string KeyIdent { get; set; } = "BaseType";
        public string Category { get; set; }

        // TierKey : Tier
        public Dictionary<string, SingleTier> FilterEntries { get; set; } = new Dictionary<string, SingleTier>();

        // Item : TierKey
        public Dictionary<string, List<string>> ItemTiering = new Dictionary<string, List<string>>();

        // Item : EcoContainer
        public Dictionary<string, EcoTier> EconomyInformation = new Dictionary<string, EcoTier>();

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

    // First we grab all entries:
    // FILTERENTRIES:   TierKey >> Entry
    // We then establish existing tiering
    // ITEMTIERING:     Item >> TierKey
    // Load Local Information
    // ITEMINFORMATION: Item >> SavedItemInformation
    // Grab online information
    // ECOTIER:         Item >> EconomyInformation

    // Iterate through all item information, process based on available tiering
    // Adjust ITEMINFORMATION/ECOTIER and create changes
    // Changes (when executed) affect ITEMTIERING
    // Iterate through ITEMTIERING, merge things into FILTERENTRIES


    //public void GetOverview()
    //{
    //    var results = this.FilterEntries
    //        .SelectMany(x => x.Value.GetLineValue<EnumValueContainer>("BaseType").Value.Select(y => y.value),
    //        (x,c) => new { key = x.Key, val = c })
    //        .ToList();
    //}
}

