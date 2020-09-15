using FilterCore;
using FilterCore.Constants;
using FilterCore.FilterComponents.Tier;
using FilterEconomy.Facades;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilterEconomyProcessor.BaseTypeMatrix
{
    public class BaseTypeMatrixFacade
    {
        private BaseTypeMatrixFacade()
        {

        }

        private static BaseTypeMatrixFacade Instance;

        public static BaseTypeMatrixFacade GetInstance()
        {
            if (Instance == null)
            {
                Instance = new BaseTypeMatrixFacade();
            }

            return Instance;
        }

        private FilterAccessFacade filterAccessFacade;
        public TierGroup FilterTiers {get; set;}
        public Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>> SortedBaseTypeMatrix { get; private set; }

        public void Initialize(FilterAccessFacade filterAccessFacade)
        {
            this.filterAccessFacade = filterAccessFacade;
            var filter = this.filterAccessFacade.PrimaryFilter;

            // access to all the relevant data in the filter
            this.FilterTiers = TierListFacade.GetInstance().TierListData["rr"];

            // dataMatrix Access
            this.SortedBaseTypeMatrix = BaseTypeDataProvider.BaseTypeTieringMatrixData;
        }
    }
}
