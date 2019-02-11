using FilterCore.FilterComponents.Tier;
using FilterEconomy.Facades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterUtilModels.Economy
{
    public interface IEconomyProcessorData
    {
        ItemInformationFacade ItemInformation { get; set; }
        EconomyRequestFacade EconomyInformation { get; set; }
        Dictionary<string, TierGroup> TierInformation { get; set; }
    }
}
