using FilterCore.Constants;
using FilterCore.Entry;
using FilterCore.FilterComponents.Tier;
using FilterCore.Line;
using FilterDomain.LineStrategy;
using FilterEconomy.Model;
using FilterEconomy.Processor;
using FilterPolishUtil;
using FilterPolishUtil.Constants;
using System;
using System.Collections.Generic;
using System.IO;
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

        public Dictionary<string, List<TieringChange>> Changelog = new Dictionary<string, List<TieringChange>>();

        // Generates simple changelogs
        private bool generatePrimitiveReport = true;

        public Dictionary<string, Dictionary<string, string>> Report { get; set; } = new Dictionary<string, Dictionary<string,string>>();
        public string WriteFolder { get; set; }

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
            this.Changelog.Clear();

            foreach (var section in this.Suggestions)
            {
                this.Changelog.Add(section.Key, new List<TieringChange>());
                this.ApplyAllSuggestionsInSection(section.Key);
            }

            var keys = this.Changelog.Keys.ToList();

            foreach (var section in keys)
            {
                this.Changelog[section] = this.Changelog[section].OrderBy(x => x.BaseType).ToList();
            }

            if (this.generatePrimitiveReport)
            {
                var report = this.GeneratePrimitiveReport();
                var seedPath = this.WriteFolder + "tierlistchanges\\" + DateTime.Today.ToString().Replace("/", "-").Replace(":", "") + ".txt";
                FileWork.WriteTextAsync(seedPath, report);
            }
        }

        public void ApplyAllSuggestionsInSection(string section)
        {
            foreach (var item in this.Suggestions[section])
            {
                this.ApplyCommand(item);
                this.Changelog[section].Add(TieringChange.FromTieringCommand(item));
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

            var oldTiers = command.OldTier.Split(',');

            var accessCommand = this.TierListData.First().Value.KeyIdent;

            foreach (var oldTier in oldTiers)
            {
                if (!FilterConstants.IgnoredSuggestionTiers.Contains(oldTier.ToLower()))
                {
                    var removalTarget = this.TierListData[command.Group]
                        .FilterEntries[oldTier].Entry
                        .Select(x => x.GetValues<EnumValueContainer>(accessCommand))
                        .SelectMany(x => x).ToList();

                    removalTarget.ForEach(x => x.Value.RemoveWhere(z => z.value.Equals(command.BaseType, StringComparison.InvariantCultureIgnoreCase)));
                    command.Performed = true;

                }
            }

            var newTiers = command.NewTier.Split(',');

            foreach (var newTier in newTiers)
            {
                if (!FilterConstants.IgnoredSuggestionTiers.Contains(newTier.ToLower()))
                {
                    var additionTarget = this.TierListData[command.Group]
                        .FilterEntries[newTier].Entry
                        .Select(x => x.GetValues<EnumValueContainer>(accessCommand))
                        .SelectMany(x => x).ToList();

                    additionTarget.ForEach(x => x.Value.Add(new LineToken() { value = command.BaseType, isQuoted = true }));
                    command.Performed = true;
                }
            }
        }

        public List<string> GeneratePrimitiveReport()
        {
            var result = new List<string>();
            foreach (var section in this.Suggestions)
            {
                if (!Report.ContainsKey(section.Key))
                {
                    Report.Add(section.Key, new Dictionary<string, string>());
                }

                foreach (var item in this.Suggestions[section.Key].Where(x => x.Performed))
                {
                    if (!Report[section.Key].ContainsKey(item.BaseType))
                    {
                        Report[section.Key].Add(item.BaseType, this.GenerateExplanation(item));
                    }
                    else
                    {
                        Report[section.Key][item.BaseType] = this.GenerateExplanation(item);
                    }
                }
            }

            foreach (var section in Report)
            {
                result.Add(string.Empty);
                result.Add("MAJOR SECTION: " + section.Key);
                result.Add(string.Empty);

                foreach (var basetypes in section.Value)
                {
                    result.Add($"{basetypes.Key}: {basetypes.Value}");
                }
                result.Add(string.Empty);
            }

            return result; //string.Join(System.Environment.NewLine, result);
        }

        private string GenerateExplanation(TieringCommand item)
        {
            return $"{item.OldTier} -> {item.NewTier}";
        }

        public void Reset()
        {
            this.Suggestions.Clear();
            this.TierListData.Clear();
            this.InitializeSuggestions();
        }
    }
}
