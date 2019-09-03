using FilterCore.Constants;
using FilterCore.Entry;
using FilterCore.FilterComponents.Tier;
using FilterCore.Line;
using FilterCore.Line.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using FilterCore.Commands.EntryCommands;

namespace FilterCore
{
    public class Filter
    {
        public List<IFilterEntry> FilterEntries { get; set; } = new List<IFilterEntry>();
        public List<IFilterLine> FilterLines { get; set; }

        public Filter(List<string> input)
        {
            List<IFilterLine> lineList = new List<IFilterLine>();

            foreach (var rawLine in input)
            {
                var tokens = LineParser.TokenizeFilterLineString(rawLine);
                var line = LineParser.GenerateFilterLine(tokens);
                lineList.Add(line);
            }
            
            this.FilterLines = lineList;

            this.GenerateFilterEntries(lineList);
        }

        private void GenerateFilterEntries(List<IFilterLine> lineList)
        {
            FilterEntries.Clear();

            FilterEntry lastDataEntry = new FilterEntry();
            FilterEntry lastCommentEntry = new FilterEntry();
            FilterConstants.FilterEntryType entryType = FilterConstants.FilterEntryType.Unknown;

            foreach (var line in lineList)
            {
                if (!string.IsNullOrEmpty(line.Ident))
                {
                    entryType = FilterConstants.FilterEntryType.Content;
                    if (line.Ident == "Show" || line.Ident == "Hide")
                    {
                        lastDataEntry = FilterEntry.CreateDataEntry(line);
                        FilterEntries.Add(lastDataEntry);
                    }
                    else
                    {
                        lastDataEntry.Content.Add(line);
                    }
                }

                else if (line.Comment != string.Empty)
                {
                    if (entryType != FilterConstants.FilterEntryType.Comment)
                    {
                        lastCommentEntry = FilterEntry.CreateCommentEntry(line);
                        entryType = FilterConstants.FilterEntryType.Comment;
                        FilterEntries.Add(lastCommentEntry);
                    }
                    else
                    {
                        lastCommentEntry.Content.AddComment(line);
                    }
                    entryType = FilterConstants.FilterEntryType.Comment;
                }

                else if (line.Comment == string.Empty)
                {
                    if (entryType == FilterConstants.FilterEntryType.Filler)
                    {
                        continue;
                    }
                    else
                    {
                        FilterEntries.Add(FilterEntry.CreateFillerEntry());
                        entryType = FilterConstants.FilterEntryType.Filler;
                    }
                }
            }
        }

        public Dictionary<string, TierGroup> ExtractTiers(HashSet<string> addressedTiers)
        {
            Dictionary<string, TierGroup> result = new Dictionary<string, TierGroup>();
            HashSet<string> foundTags = new HashSet<string>();

            // analyze every entry...
            foreach (var entry in this.FilterEntries)
            {
                //..with tiertags
                if (entry.Header.TierTags != null)
                {
                    if (!entry.Header.TierTags.ContainsKey("type") || !entry.Header.TierTags.ContainsKey("tier"))
                    {
                        continue;
                    }

                    var primaryTag = entry.Header.TierTags["type"].CombinedTagValue.ToLower();

                    if (addressedTiers.Contains(primaryTag))
                    { 
                        var tier = entry.Header.TierTags["tier"].CombinedTagValue.ToLower();

                        // füge eine neue Hauptgruppe hinzu
                        if (!result.ContainsKey(primaryTag))
                        {
                            result.Add(primaryTag, new TierGroup(primaryTag));
                        }

                        if (!result[primaryTag].FilterEntries.ContainsKey(tier))
                        {
                            result[primaryTag].FilterEntries[tier] = new SingleTier() { TierName = tier };
                        }

                        result[primaryTag].FilterEntries[tier].Entry.Add(entry);
                    }
                }
            }

            return result;
        }

        public void ExecuteCommandTags()
        {
            for (var i = 0; i < this.FilterEntries.Count; i++)
            {
                var filterEntry = this.FilterEntries[i];
                if (filterEntry.Header.Type != FilterConstants.FilterEntryType.Content)
                {
                    continue;
                }

                if (filterEntry.Header.IsFrozen)
                {
                    continue;
                }

                ExecuteCommandTags_Inner(filterEntry);
                
                // local func
                void ExecuteCommandTags_Inner(IFilterEntry entry)
                {
                    int? index = null;
                    foreach (var command in entry.Header.GenerationTags)
                    {
                        if (command.IsStrictnessCommand()) continue;
                        command.Execute();

                        if (command is IEntryGenerationCommand cmd)
                        {
                            if (!index.HasValue) index = this.FilterEntries.IndexOf(entry);

                            var newEntries = cmd.NewEntries;
                            this.InsertEntries(index.Value, newEntries);
                            i += newEntries.ToList().Count * 2; // *2 because of the filler entries/lines in between
                            newEntries.ToList().ForEach(ExecuteCommandTags_Inner);
                        }
                    }
                }
            }
        }

        public void ExecuteStrictnessCommands(int strictnessIndex, int? consoleStrictness)
        {
            foreach (var entry in this.FilterEntries)
            {
                if (entry.Header.Type != FilterConstants.FilterEntryType.Content)
                {
                    continue;
                }

                if (entry.Header.IsFrozen)
                {
                    continue;
                }

                foreach (var command in entry.Header.GenerationTags)
                {
                    if (!command.IsStrictnessCommand()) continue;
                    command.Execute(strictnessIndex, consoleStrictness);
                }
            }
            
            this.SetHeaderMetaData("type:", strictnessIndex + "-" + FilterConstants.FilterStrictnessLevels[strictnessIndex].ToUpper());
        }

        public void InsertEntries(int index, IEnumerable<IFilterEntry> newEntries)
        {
            var list = newEntries.ToList();
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (i == 0) break;
                list.Insert(i, FilterEntry.CreateFillerEntry());
            }
            list.Add(FilterEntry.CreateFillerEntry());
            
            this.FilterEntries.InsertRange(index, list);
        }

        public List<string> Serialize()
        {
            var results = new List<string>();
            this.FilterEntries.ForEach(x => results.AddRange(x.Serialize()));
            return results;
        }

        public string GetHeaderMetaData(string key)
        {
            return this.GetHeaderMetaDataLine(key).Comment.Split('\t', ' ').Last();
        }

        public void SetHeaderMetaData(string key, string newVersion)
        {
            var line = this.GetHeaderMetaDataLine(key);
            var oldValue = line.Comment.Trim().Split(' ', '\t').Last();
            line.Comment = line.Comment.Replace(oldValue, newVersion);
        }
        
        private IFilterLine GetHeaderMetaDataLine(string key)
        {
            foreach (var entry in this.FilterEntries)
            {
                if (entry.Header.Type != FilterConstants.FilterEntryType.Comment) continue;
                foreach (var line in entry.Content.Content["comment"])
                {
                    var comment = line.Comment;
                    if (!comment.ToLower().Contains(key.ToLower())) continue;
                    return line;
                }
            }

            throw new Exception("version entry not found");
        }
    }
}
