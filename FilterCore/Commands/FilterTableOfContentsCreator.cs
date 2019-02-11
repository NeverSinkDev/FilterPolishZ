using System;
using System.Collections.Generic;
using System.Linq;
using FilterCore.Constants;
using FilterCore.Entry;
using FilterCore.Line;
using FilterCore.Line.LineStrategy;
using FilterPolishUtil;

namespace FilterCore.Commands
{
    public class FilterTableOfContentsCreator
    {
        // (existing) filterEntry, containing the ToC strings as comments - this is where the result will be inserted into
        private IFilterEntry filterEntry;
        
        private readonly FilterTableOfContentsInfo tableContent = new FilterTableOfContentsInfo("root", -1, StartIndex);
        private readonly Filter filter;
        
        // actual constants - should not change unless filter code style changes
        private const int MinSectionTitleLineCount = 3; 
        private const int SectionTitleLineIndex = 1; // index of the line in the filterEntry that contains the title
        private const char SectionTitleKeyIdentStart = '[';
        private const char SectionTitleKeyIdentEnd = ']';
        
        // customization options
        private const int MaxDepth = 2;
        private const int IndexDigitCount = 2;
        private const int StartIndex = 1;

        public FilterTableOfContentsCreator(Filter filter)
        {
            this.filter = filter;
            this.Run();
        }

        public void Run()
        {
            this.filterEntry = this.FindFilterEntry();
            this.ScanFilterContent();
            this.WriteTableIntoEntry();
        }

        private IFilterEntry FindFilterEntry()
        {
            foreach (var entry in this.filter.FilterEntries)
            {
                if (!this.IsSectionTitleEntry(entry)) continue;
                
                if (entry.Content.Content["comment"][1].Comment.Contains("[WELCOME]"))
                {
                    return entry;
                }
            }
            
            throw new Exception("unable to find ToC entry");
        }

        private void ScanFilterContent()
        {
            foreach (var entry in this.filter.FilterEntries)
            {
                if (!this.IsSectionTitleEntry(entry))
                {
                    continue;
                }

                var line = this.GetTitleLineFromEntry(entry);
                var depth = this.GetTitleDepth(line);
                var title = this.GetTitle(line, depth);
                
                this.AddNewSection(title, depth);
            }
        }

        private string GetTitle(string line, int depth)
        {
            var expectedKeyEnd = SectionTitleKeyIdentEnd.ToString().Times(depth);
            var index = line.IndexOf(expectedKeyEnd, StringComparison.Ordinal);
            index += expectedKeyEnd.Length;
            return line.Substring(index);
        }

        private int GetTitleDepth(string line)
        {
            var bracketCount = 0;
            foreach (var c in line)
            {
                if (c == SectionTitleKeyIdentStart) bracketCount++;
                else break;
            }
            return bracketCount;
        }
        
        private bool IsSectionTitleEntry(IFilterEntry entry)
        {
            if (entry.Header.Type != FilterConstants.FilterEntryType.Comment)
            {
                return false;
            }

            if (entry == this.filterEntry)
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

            var line = this.GetTitleLineFromEntry(entry);
            if (line[0] != SectionTitleKeyIdentStart || !line.Contains(SectionTitleKeyIdentEnd))
            {
                return false;
            }

            return true;
        }

        private string GetTitleLineFromEntry(IFilterEntry entry)
        {
            return entry.Content.Content["comment"][SectionTitleLineIndex].Comment.Trim();
        }

        private void AddNewSection(string name, int depth)
        {
            // the more brackets, the less deep the section is -> we invert that number here
            depth = MaxDepth - depth;
            
            this.tableContent.InsertTitle(name, depth);
        }

        private void WriteTableIntoEntry()
        {
            var content = this.tableContent.Stringify();
            var lines = new List<IFilterLine>(content.Select(x => new FilterLine<EmptyValueContainer> {Comment = x}));
            this.filterEntry.Content.Content["comment"] = lines;
        }

        private class FilterTableOfContentsInfo
        {
            private int Depth { get; }
            private readonly List<FilterTableOfContentsInfo> content;
            private string Title { get; }
            private readonly int index;

            public FilterTableOfContentsInfo(string title, int depth, int index)
            {
                this.Title = title;
                this.Depth = depth;
                this.index = index;
                this.content = new List<FilterTableOfContentsInfo>();
            }

            public void InsertTitle(string title, int depth)
            {
                if (depth == 0)
                {
                    this.content.Insert(this.content.Count, new FilterTableOfContentsInfo(title, this.Depth+1, this.content.Count+StartIndex));
                    return;
                }
                
                this.content[this.content.Count-1].InsertTitle(title, depth-1);
            }

            public List<string> Stringify(string prefix = "")
            {
                var res = new List<string>();
                var idx = this.index.ToString().PadLeft(IndexDigitCount, '0');

                // skip the -1 root level title
                if (this.Depth >= 0)
                {
                    var bracketCount = MaxDepth - this.Depth;
                    var spaceCount = this.Depth * 2; // left side spaces that make all numbers end on the same level

                    var line = "";
                    line += " ".Times(spaceCount);
                    line += SectionTitleKeyIdentStart.ToString().Times(bracketCount);
                    line += prefix;
                    line += idx;
                    line += "".PadRight((bracketCount - 1) * IndexDigitCount, '0');
                    line += SectionTitleKeyIdentEnd.ToString().Times(bracketCount);
                    line += " " + this.Title;

                    res.Add("# " + line);
                }
                else idx = "";
                
                this.content.ForEach(x => res.AddRange(x.Stringify(prefix + idx)));
                
                return res;
            }
        }
    }
}