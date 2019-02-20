using FilterEconomy.Model.ItemAspects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterEconomy.Model.ItemInformationData
{
    public class ItemInformationData
    {
        public string Name { get; set; }
        public string BaseType { get; set; }
        public string Special { get; set; }

        public List<IItemAspect> Aspects { get; set; } = new List<IItemAspect>();

        public NinjaItem ToNinjaItem()
        {
            return new NinjaItem
            {
                Name = this.Name,
                BaseType = this.BaseType,
                Variant = this.Special,
                Aspects = new ObservableCollection<IItemAspect>(this.Aspects)
            };
        }
    }
}
