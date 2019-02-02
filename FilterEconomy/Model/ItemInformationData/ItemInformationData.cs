using FilterEconomy.Model.ItemInformationData.ItemAspect;
using System;
using System.Collections.Generic;
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
    }
}
