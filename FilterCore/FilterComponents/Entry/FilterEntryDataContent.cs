using System;
using FilterCore.Constants;
using FilterCore.Line;
using System.Collections.Generic;
using System.Linq;
using FilterDomain.LineStrategy;

namespace FilterCore.Entry
{
    public class FilterEntryDataContent : IFilterEntryContent
    {
        public Dictionary<string, List<IFilterLine>> Content { get; set; }

        public bool Add(IFilterLine line)
        {
            if (!this.Content.ContainsKey(line.Ident))
            {
                this.Content.Add(line.Ident, new List<IFilterLine>());
                this.Content[line.Ident].Add(line);
                return true;
            }

            this.Content[line.Ident].Add(line);
            return false;
        }

        public void AddComment(IFilterLine line)
        {
            if (!this.Content.ContainsKey("comment"))
            {
                this.Content.Add("comment", new List<IFilterLine>());
            }

            this.Content["comment"].Add(line);
        }

        public void AddComment(string line)
        {
            if (!this.Content.ContainsKey("comment"))
            {
                this.Content.Add("comment", new List<IFilterLine>());
            }

            this.Content["comment"].Add(line.ToFilterLine());
        }

        public bool Has(string ident)
        {
            return this.Content.ContainsKey(ident);
        }

        public bool Has(IFilterLine line)
        {
            return this.Has(line.Ident);
        }

        public List<IFilterLine> Get(IFilterLine line)
        {
            return this.Get(line.Ident);
        }

        public List<IFilterLine> Get(string ident)
        {
            if (this.Content.ContainsKey(ident))
            {
                return this.Content[ident];
            }

            return null;
        }

        public IFilterLine GetFirst(IFilterLine line)
        {
            return this.GetFirst(line.Ident);
        }

        public IFilterLine GetFirst(string ident)
        {
            if (this.Content.ContainsKey(ident))
            {
                return this.Content[ident].FirstOrDefault();
            }

            return null;
        }

        public IFilterLine GetFirstLineWhere(Func<IFilterLine, bool> filterFunc)
        {
            foreach (var lineList in this.Content.Values)
            {
                foreach (var line in lineList)
                {
                    if (filterFunc.Invoke(line))
                    {
                        return line;
                    }
                }
            }

            return null;
        }

        public void Revert()
        {

        }

        public void RemoveAll(string ident)
        {
            if (this.Content.ContainsKey(ident))
            {
                this.Content.Remove(ident);
            }
        }

        public List<string> Serialize()
        {
            return this.Content
                .SelectMany(x => x.Value.Select(z => new { Ident = z.Ident, SerializedString = z.Serialize(), IsActive = z.IsActive }))
                .Where(x => x.IsActive)
                .OrderBy(x => FilterGenerationConfig.LineTypesSort[x.Ident])
                .Select(x => x.SerializedString)
                .ToList();
        }

        public List<string> DebugRawValues()
        {
            return this.Content
                .SelectMany(x => x.Value.Select(z => new { Ident = z.Ident, SerializedString = z.Serialize(), IsActive = z.IsActive }))
                .Where(x => x.IsActive)
                .OrderBy(x => FilterGenerationConfig.LineTypesSort[x.Ident])
                .Select(x => x.SerializedString)
                .ToList();
        }

        public FilterEntryDataContent Clone()
        {
            var cloneContent = new Dictionary<string, List<IFilterLine>>();

            foreach (var ident in this.Content)
            {
                var list = new List<IFilterLine>();
                
                foreach (var line in ident.Value)
                {
                    list.Add(line.Clone());
                }
                
                cloneContent.Add(ident.Key, list);
            }
            
            return new FilterEntryDataContent
            {
                Content = cloneContent
            };
        }

        public void UpdateParent(FilterEntry newParent)
        {
            foreach (var ident in this.Content)
            {
                foreach (var filterLine in ident.Value)
                {
                    if (filterLine is FilterLine<ILineValueCore> line)
                    {
                        line.Parent = newParent;
                    }
                }
            }
        }
    }
}
