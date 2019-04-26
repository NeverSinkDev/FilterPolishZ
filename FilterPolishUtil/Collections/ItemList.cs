using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishUtil.Collections
{
    public class ItemList<T> : List<T>
    {
        public float HighestPrice { get; set; }
        public float LowestPrice { get; set; }
        public float ValueMultiplier { get; set; } = 1;

        public bool Valid { get; set; } = true;

        public AutoDictionary<int, float> ftCount { get; set; }
        public AutoDictionary<int, float> ftPrice { get; set; }
    }
}
