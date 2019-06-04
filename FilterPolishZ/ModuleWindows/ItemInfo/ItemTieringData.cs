using FilterEconomy.Model;
using FilterPolishUtil.Collections;

namespace FilterPolishZ.ModuleWindows.ItemInfo
{
    public class ItemTieringData
    {
        public string Name { get; set; }
        public ItemList<NinjaItem> Value { get; set; } = new ItemList<NinjaItem>();
        public float LowestPrice { get; set; } = 0;
        public float Count { get; set; } = 0;
        public float Multiplier { get; internal set; }
        public float HighestPrice { get; internal set; }
        public bool? Valid { get; internal set; }
    }
}
