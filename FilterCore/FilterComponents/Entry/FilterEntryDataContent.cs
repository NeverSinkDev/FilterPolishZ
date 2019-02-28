using FilterCore.Constants;
using FilterCore.Line;
using System.Collections.Generic;
using System.Linq;

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
                .SelectMany(x => x.Value.Select(z => new { Ident = z.Ident, SerializedString = z.Serialize() }))
                .OrderBy(x => FilterConstants.LineTypesSort[x.Ident])
                .Select(x => x.SerializedString)
                .ToList();
        }
    }
}
