using FilterCore.Constants;
using FilterCore.Entry;
using FilterCore.FilterComponents.Tier;
using FilterCore.Line;
using FilterDomain.LineStrategy;
using FilterEconomy.Processor;
using FilterPolishUtil.Constants;
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
            this.InitializeSuggestions();
        }

        public void InitializeSuggestions()
        {
            foreach (var item in FilterPolishConstants.FilterTierLists)
            {
                this.Suggestions.Add(item, new List<TieringCommand>());
            }
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

        public void ApplyAllSuggestions()
        {
            foreach (var section in this.Suggestions)
            {
                this.ApplyAllSuggestionsInSection(section.Key);
            }
        }

        public void ApplyAllSuggestionsInSection(string section)
        {
            foreach (var item in this.Suggestions[section])
            {
                this.ApplyCommand(item);
            }
        }

        public void ApplyCommand(TieringCommand command)
        {

            if (command.Performed)
            {
                return;
            }

            if (command.OldTier.ToLower() == command.NewTier.ToLower())
            {
                return;
            }

            if (command.NewTier == "???")
            {
                return;
            }

            if (!FilterConstants.IgnoredSuggestionTiers.Contains(command.OldTier.ToLower()))
            {
                var removalTarget = this.TierListData[command.Group]
                    .FilterEntries[command.OldTier].Entry
                    .Select(x => x.GetValues<EnumValueContainer>("BaseType"))
                    .SelectMany(x => x).ToList();

                removalTarget.ForEach(x => x.Value.RemoveWhere(z => z.value.Equals(command.BaseType, StringComparison.InvariantCultureIgnoreCase)));
                command.Performed = true;

            }

            var newTiers = command.NewTier.Split(',');

            foreach (var newTier in newTiers)
            {
                if (!FilterConstants.IgnoredSuggestionTiers.Contains(newTier.ToLower()))
                {
                    var additionTarget = this.TierListData[command.Group]
                        .FilterEntries[newTier].Entry
                        .Select(x => x.GetValues<EnumValueContainer>("BaseType"))
                        .SelectMany(x => x).ToList();

                    additionTarget.ForEach(x => x.Value.Add(new LineToken() { value = command.BaseType, isQuoted = true }));
                    command.Performed = true;
                }
            }
        }

        public void Reset()
        {
            this.Suggestions.Clear();
            this.TierListData.Clear();
            this.InitializeSuggestions();
        }
    }
}
