﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPolishUtil.Collections
{
    public class ItemList<T> : List<T>
    {
        public AutoDictionary<string, string> MetaData { get; set; } = new AutoDictionary<string, string>();
        
        public float HighestPrice { get; set; }
        public float LowestPrice { get; set; }
        public float ValueMultiplier { get; set; } = 1;
    }
}
