using FilterEconomy.Model.ItemAspects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterEconomy.Model
{
    public class NinjaItem
    {
        [JsonProperty("itemType")]
        public string Class { get; set; }

        [JsonProperty("baseType")]
        public string BaseType { get; set; }

        [JsonProperty("links")]
        public float Links { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("variant")]
        public string Variant { get; set; }

        [JsonProperty("explicitModifiers")]
        public string ExplicitModifiers { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("chaosValue")]
        public float CVal { get; set; }

        [JsonProperty("exaltedValue")]
        public float XVal { get; set; }

        [JsonProperty("levelRequired")]
        public float LevelRequired { get; set; }

        [JsonProperty("itemClass")]
        public float ItemClass { get; set; }

        [JsonProperty("count")]
        public float IndexedCount { get; set; }

        [JsonProperty("stackSize")]
        public int StackSize { get; set; }

        //[JsonProperty("sparkline")]
        //public int[] SparkLine {get; set;}

        //[JsonProperty("lowConfidenceSparkline")]
        //public int[] LowConfSparkLine { get; set; }

        public ObservableCollection<IItemAspect> Aspects { get; set; } = new ObservableCollection<IItemAspect>();

        public bool isItemRelic()
        {
            return this.Icon.Contains("relic=1");
        }

        // this item was manually created because it was missing from the (poeNinja) data
        public bool IsVirtual { get; set; }
    }
}
