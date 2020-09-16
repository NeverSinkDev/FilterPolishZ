using FilterCore;
using FilterCore.Entry;
using FilterCore.FilterComponents.Tier;
using FilterCore.Line;
using FilterDomain.LineStrategy;
using FilterEconomy.Model;
using FilterEconomy.Processor;
using FilterPolishUtil;
using FilterPolishUtil.Extensions;
using FilterPolishUtil.Interfaces;
using FilterPolishUtil.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FilterEconomy.Facades
{
    public class TierListFacade : ICleanable
    {
        private static TierListFacade Instance { get; set; }
        public Dictionary<string, TierGroup> TierListData { get; set; } = new Dictionary<string, TierGroup>();
        public Dictionary<string, List<TieringCommand>> Suggestions = new Dictionary<string, List<TieringCommand>>();
        public Dictionary<string, bool> EnabledSuggestions = new Dictionary<string, bool>();

        public Dictionary<string, List<TieringChange>> Changelog = new Dictionary<string, List<TieringChange>>();

        // Generates simple changelogs
        private bool generatePrimitiveReport = false;

        public Dictionary<string, Dictionary<string, string>> Report { get; set; } = new Dictionary<string, Dictionary<string,string>>();
        public string WriteFolder { get; set; }

        private TierListFacade()
        {
            this.InitializeSuggestions();
        }

        public void InitializeSuggestions()
        {
            foreach (var item in FilterPolishConfig.FilterTierLists)
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
            if (group == "basetypes")
            {
                group = "generalcrafting";
            }

            return TierListData[group].ItemTiering.ContainsKey(basetype);
        }

        public List<string> GetTiersForBasetype(string group, string basetype)
        {
            if (!TierListData[group].ItemTiering.ContainsKey(basetype))
            {
                return new List<string>();
            }

            return TierListData[group].ItemTiering[basetype];
        }

        public List<string> GetTiersForBasetypeSafe(string group, string basetype)
        {
            if (!this.ContainsTierInformationForBaseType(group,basetype))
            {
                return new List<string> { "untiered!" };
            }

            return TierListData[group].ItemTiering[basetype];
        }

        public void ApplyAllSuggestions()
        {
            LoggingFacade.LogInfo("Applying All Suggestions!");
            this.Changelog.Clear();

            foreach (var section in this.Suggestions)
            {
                if (!this.TierListData.ContainsKey(section.Key) || (this.TierListData[section.Key].FilterEntries?.Count == 0))
                {
                    if (FilterPolishConfig.AutoTieringIgnoredTiers.Contains(section.Key))
                    {
                        continue;
                    }

                    LoggingFacade.LogWarning($"Skipping section {section.Key} . No data found for section!");
                    continue;
                }

                if (this.EnabledSuggestions.ContainsKey(section.Key) && !this.EnabledSuggestions[section.Key])
                {
                    LoggingFacade.LogInfo($"SKIP Suggestions generation for: {section.Key}");
                    continue;
                }

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
            LoggingFacade.LogDebug($"Applying Suggestions in section: {section}");

            this.AddNonBaseTypeExceptionList(section);
            this.AddBaseTypeLinesIfMissing(section);

            foreach (var item in this.Suggestions[section])
            {
                this.ApplyCommand(item);
                this.Changelog[section].Add(TieringChange.FromTieringCommand(item));
            }

            this.RemoveBaseTypeLinesIfEmpty(section);
            this.HandleEnabledDisabledState(section);
        }

        private void AddNonBaseTypeExceptionList(string section)
        {
            var ident = this.TierListData[section].KeyIdent;
            this.TierListData[section].FilterEntries
                .SelectMany(x => x.Value.Entry).ToList()
                .ForEach(x =>
                {
                    if (!x.HasLine<EnumValueContainer>(ident) && x.Header.IsFrozen == false)
                    {
                        this.TierListData[section].NonBaseTypeEntries.AddIfNew(x.Header.TierTags.Serialize());
                    }
                });
        }

        private void HandleEnabledDisabledState(string section)
        {
            var ident = this.TierListData[section].KeyIdent;

            this.TierListData[section].FilterEntries
                .SelectMany(x => x.Value.Entry)
                .Where(x => !this.TierListData[section].NonBaseTypeEntries.Contains(x.Header.TierTags.Serialize())).ToList()
                .ForEach(x =>
                {
                    if (x.HasLine<EnumValueContainer>(ident))
                    {
                        x.SetEnabled(true);
                    }
                    else
                    {
                        x.SetEnabled(false);
                        LoggingFacade.LogDebug($"Disabling empty entry: {x.Header.TierTags.Serialize()}");
                    }
                });
        }

        private void RemoveBaseTypeLinesIfEmpty(string section)
        {
            var ident = this.TierListData[section].KeyIdent;

            this.TierListData[section].FilterEntries
                .SelectMany(x => x.Value.Entry)
                .Where(x => !x.GetLines<EnumValueContainer>(ident).Any(z => z.Value.IsValid()) 
                    && !this.TierListData[section].NonBaseTypeEntries.Contains(x.Header.TierTags.Serialize())).ToList()
                .ForEach(x =>
                {
                    x.Content.RemoveAll(ident);
                    LoggingFacade.LogDebug($"Removing Empty BaseType Line: {x.Header.TierTags.Serialize()}");
                });
        }

        private void AddBaseTypeLinesIfMissing(string section)
        {
            var ident = this.TierListData[section].KeyIdent;

            this.TierListData[section].FilterEntries
                .SelectMany(x => x.Value.Entry)
                .Where(x => !x.HasLine<EnumValueContainer>(ident) 
                    && !this.TierListData[section].NonBaseTypeEntries.Contains(x.Header.TierTags.Serialize())).ToList()
                .ForEach(x =>
                {
                    x.Content.Content.Add(ident, new List<IFilterLine>
                        {
                            new FilterLine<EnumValueContainer>
                            {
                                Ident = ident,
                                Parent = x,
                                Value = new EnumValueContainer()
                            }
                        });

                    LoggingFacade.LogDebug($"Adding Missing BaseType Line: {x.Header.TierTags.Serialize()}");
                });
        }

        public void ApplyCommand(TieringCommand command)
        {
            if (command.LocalIgnore)
            {
                return;
            }

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

            var accessCommand = this.TierListData[command.Group].KeyIdent;

            foreach (var oldTier in oldTiers)
            {
                if (!FilterGenerationConfig.IgnoredSuggestionTiers.Contains(oldTier.ToLower()))
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
                if (!FilterGenerationConfig.IgnoredSuggestionTiers.Contains(newTier.ToLower()))
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

        public void Clean()
        {
            this.Reset();
            this.Changelog.Clear();
            this.Report.Clear();
            Instance = null;
        }
    }
}
