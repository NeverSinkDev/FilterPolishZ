using FilterCore.Constants;
using FilterCore.Entry;
using FilterCore.FilterComponents.Tier;
using FilterCore.Line;
using FilterDomain.LineStrategy;
using FilterEconomy.Processor;
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
        public Dictionary<string, List<TieringCommand>> Suggestions = new Dictionary<string, List<TieringCommand>>();

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

        public void ApplyCommand(TieringCommand command)
        {

            if (command.Change)
            {
                return;
            }

            if (command.OldTier.ToLower() == command.NewTier.ToLower())
            {
                return;
            }

            if (command.NewTier == "???")
            {
                command.Unsure = true;
                return;
            }

            if (!FilterConstants.IgnoredSuggestionTiers.Contains(command.OldTier.ToLower()))
            {
                var removalTarget = this.TierListData[command.Group]
                    .FilterEntries[command.OldTier].Entry
                    .Select(x => x.GetValues<EnumValueContainer>("BaseType"))
                    .SelectMany(x => x).ToList();

                removalTarget.ForEach(x => x.Value.RemoveWhere(z => z.value.Equals(command.BaseType, StringComparison.InvariantCultureIgnoreCase)));
                command.Change = true;

            }

            if (!FilterConstants.IgnoredSuggestionTiers.Contains(command.NewTier.ToLower()))
            {
                var additionTarget = this.TierListData[command.Group]
                    .FilterEntries[command.NewTier].Entry
                    .Select(x => x.GetValues<EnumValueContainer>("BaseType"))
                    .SelectMany(x => x).ToList();

                additionTarget.ForEach(x => x.Value.Add(new LineToken() { value = command.BaseType, isQuoted = true }));
                command.Change = true;
            }
        }
    }
}
