using FilterCore.FilterComponents.Tier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterEconomy.Facades
{
    public class TierListFacade
    {
        public static TierListFacade Instance { get; set; }
        public Dictionary<string, TierGroup> TierListData { get; set; } = new Dictionary<string, TierGroup>();

        private TierListFacade()
        {

        }

        public static TierListFacade GetInstance()
        {
            if (Instance == null)
            {
                Instance = new TierListFacade();
            }
            return Instance;
        }
    }
}
