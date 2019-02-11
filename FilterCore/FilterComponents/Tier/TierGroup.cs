using System.Collections.Generic;

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
        public Dictionary<string, string> ItemTiering = new Dictionary<string, string>();

        // Item : EcoContainer
        public Dictionary<string, EcoTier> EconomyInformation = new Dictionary<string, EcoTier>(); 

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
}
