using FilterPolishUtil.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterCore
{
    public class FilterAccessFacade : ICleanable
    {
        private static FilterAccessFacade instance;
        private FilterAccessFacade()
        {
            this.FilterStorage.Add("primary", null);
        }

        public Dictionary<string, Filter> FilterStorage { get; set; } = new Dictionary<string, Filter>();
        public Filter PrimaryFilter
        {
            get => FilterStorage["primary"];
            set => FilterStorage["primary"] = value;
        }

        public static FilterAccessFacade GetInstance()
        {
            if (instance == null)
            {
                instance = new FilterAccessFacade();
            }

            return instance;
        }

        public void Clean()
        {
            FilterStorage.Clear();
            instance = null;
        }
    }
}
