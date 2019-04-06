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
        private static TierListFacade Instance { get; set; }
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

        public bool ContainsTierInformationForBaseType(string group, string basetype)
        {
            return TierListData[group].ItemTiering.ContainsKey(basetype);
        }

        public List<string> GetTiersForBasetype(string group, string basetype)
        {
            return TierListData[group].ItemTiering[basetype];
        }
    }
}
