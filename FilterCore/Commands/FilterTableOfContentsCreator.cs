using System;
using System.Collections.Generic;
using System.Linq;
using FilterCore.Constants;
using FilterCore.Entry;
using FilterCore.Line;
using FilterCore.Line.LineStrategy;
using FilterPolishUtil;
using FilterPolishUtil.Collections;

namespace FilterCore.Commands
{
    public class FilterTableOfContentsCreator
    {
        // (existing) filterEntry, containing the ToC strings as comments - this is where the result will be inserted into
        private IFilterEntry filterEntry;
        private readonly List<int> indices = new List<int>();
        private readonly List<string> tableLines = new List<string>();
        private readonly Filter filter;
        
        // actual constants - should not change unless filter code style changes
        private const int MinSectionTitleLineCount = 3; 
        private const int SectionTitleLineIndex = 1; // index of the line in the filterEntry that contains the title
        private const char SectionTitleKeyIdentStart = '[';
        private const char SectionTitleKeyIdentEnd = ']';
        private const int StaticTableHeaderLineCount = 4; // 1st = divider, 2nd = [WELCOME], 3rd = divider, 4th = filler
        
        // customization options
        private const int MaxDepth = 2;
        private const int IndexDigitCount = 2;
        private const int StartIndex = 1;

        public FilterTableOfContentsCreator(Filter filter)
        {
            this.filter = filter;
        }

        public void Run()
        {
            this.filterEntry = this.FindFilterEntry();
            this.SaveTableHeaderLines();
            this.ScanFilterContent();
            this.WriteTableIntoEntry();
        }

        private void SaveTableHeaderLines()
        {
            var headerLines = this.filterEntry.Content.Content["comment"].GetRange(0, StaticTableHeaderLineCount);
            var headerStrings = headerLines.Select(x => x.Comment);
            this.tableLines.AddRange(headerStrings);
        }

        private IFilterEntry FindFilterEntry()
        {
            foreach (var entry in this.filter.FilterEntries)
            {
                if (entry.Header.Type != FilterGenerationConfig.FilterEntryType.Comment)
                {
                    continue;
                }

                if (entry.Content.Content.ContainsKey("comment") && entry.Content.Content["comment"].Count > 2)
                {
                    if (entry.Content.Content["comment"][1].Comment.Contains("[WELCOME]"))
                    {
                        return entry;
                    }
                }
            }
            
            throw new Exception("unable to find ToC entry");
        }

        private void ScanFilterContent()
        {
            foreach (var entry in this.filter.FilterEntries)
            {
                if (!IsSectionTitleEntry(entry))
                {
                    continue;
                }

                var line = GetTitleLineFromEntry(entry);
                var depth = GetTitleDepth(line.Comment);
                var title = GetTitle(line.Comment, depth);
                
                this.AddNewSection(title, depth, line);
            }
        }
        
        private static IFilterLine GetTitleLineFromEntry(IFilterEntry entry) => entry.Content.Content["comment"][SectionTitleLineIndex];

        public static int GetTitleDepth(IFilterEntry entry) => GetTitleDepth(GetTitleLineFromEntry(entry).Comment);
        public static int GetTitleDepth(string line)
        {
            var bracketCount = 0;
            foreach (var c in line.Trim())
            {
                if (c == SectionTitleKeyIdentStart) bracketCount++;
                else break;
            }
            if (bracketCount > MaxDepth) throw new Exception("ToC error: found title line exceeding maxDepth");
            return bracketCount;
        }

        public static string GetTitle(IFilterEntry entry, int depth) => GetTitle(GetTitleLineFromEntry(entry).Comment, depth);
        public static string GetTitle(string line, int depth)
        {
            var expectedKeyEnd = SectionTitleKeyIdentEnd.ToString().Times(depth);
            var index = line.IndexOf(expectedKeyEnd, StringComparison.Ordinal);
            index += expectedKeyEnd.Length;
            return line.Substring(index);
        }

        private void AddNewSection(string name, int depth, IFilterLine line)
        {
            // the more brackets, the less deep the section is -> we invert that number here
            depth = MaxDepth - depth;
            
            var index = this.AddIndex(depth);
            var prefix = this.GetPrefix(depth);
            var title = this.BuildLineTitle(depth, index, name, prefix);
            line.Comment = title;
            this.tableLines.Add(title);
        }
        
        private int AddIndex(int depth)
        {
            // increase list size for first entry of this depth
            if (depth > this.indices.Count - 1)
            {
                var missingSlots = depth - (this.indices.Count - 1);
                this.indices.AddRange(Enumerable.Repeat(StartIndex-1, missingSlots));
            }

            // revert deeper indices so that they re-start at zero when they're used again
            for (var i = depth + 1; i < this.indices.Count; i++)
            {
                this.indices[i] = StartIndex-1;
            }

            // first increase, THEN return. else the children will have the increased index in their prefix
            // thats why we start with StartIndex-1
            return ++this.indices[depth];
        }
        
        // prefix = the indices of the "parent" sections. for "[1502] Top Gems", the prefix is "15", because the parent section is "[[1500]] Gems"
        private string GetPrefix(int depth)
        {
            var prefix = "";
            for (var i = 0; i < depth; i++)
            {
                prefix += this.indices[i].ToString().PadLeft(IndexDigitCount, '0');
            }
            return prefix;
        }

        private string BuildLineTitle(int depth, int index, string title, string prefix)
        {
            var bracketCount = MaxDepth - depth;
            var spaceCount = depth * 2; // left side spaces that make all numbers end on the same level
            
            if (index.ToString().Length > IndexDigitCount) throw new Exception("ToC index gets too high for selected indexDigitCount");

            var line = "";
            line += " ".Times(spaceCount);                                     // ident spaces
            line += SectionTitleKeyIdentStart.ToString().Times(bracketCount);  // open brackets
            line += prefix;                                                    // indices of the parents
            line += index.ToString().PadLeft(IndexDigitCount, '0');            // current index with zeros filled on the left
            line += "".PadRight((bracketCount - 1) * IndexDigitCount, '0');    // fill in children-zeros. for "[[0400]] ELDER" the 2 zeros to the right of the 4
            line += SectionTitleKeyIdentEnd.ToString().Times(bracketCount);    // close brackets
            line += title;                                                     // actual title of the section

            return line;
        }

        private void WriteTableIntoEntry()
        {
            var lines = new List<IFilterLine>(this.tableLines.Select(x => new FilterLine<EmptyValueContainer> {Comment = x}));
            this.filterEntry.Content.Content["comment"] = lines;
        }
        
        public static bool IsSectionTitleEntry(IFilterEntry entry, IFilterEntry tocEntry = null)
        {
            if (entry.Header.Type != FilterGenerationConfig.FilterEntryType.Comment)
            {
                return false;
            }
            
            if (!entry.Content.Content.ContainsKey("comment"))
            {
                return false;
            }
            
            if (entry.Content.Content["comment"].Count < MinSectionTitleLineCount)
            {
                return false;
            }

            var line = GetTitleLineFromEntry(entry);
            
            if (line.Comment.Trim()[0] != SectionTitleKeyIdentStart || !line.Comment.Contains(SectionTitleKeyIdentEnd))
            {
                return false;
            }
            
            // skip the actual ToC entry, even tho it does look fitting
            if (entry == tocEntry || line.Comment.ToUpper().Contains("[WELCOME]"))
            {
                return false;
            }

            bool IsDivider(IFilterLine l) => l.Comment.Length > 5 && l.Comment[2] == l.Comment[3] && l.Comment[3] == l.Comment[4];
            if (!IsDivider(entry.Content.Content["comment"][0]) || !IsDivider(entry.Content.Content["comment"][0]))
            {
                return false;
            }

            return true;
        }
    }
}