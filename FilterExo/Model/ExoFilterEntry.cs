using FilterCore.Entry;
using FilterExo.Core.Process.Commands;
using FilterExo.Core.Structure;
using FilterPolishUtil.Model;
using System;
using System.Collections.Generic;
using System.Text;
using static FilterExo.FilterExoConfig;

namespace FilterExo.Model
{
    public class ExoFilterEntry
    {
        // Identifying elements
        public ExoFilterType Type;
        public string Command;
        public int ID;
        public string Name;

        // Hierarchical elements
        public ExoFilterEntry Parent;

        public List<ExoFilterEntry> Entries = new List<ExoFilterEntry>();
        public Dictionary<string, ExoFilterEntry> Registry = new Dictionary<string, ExoFilterEntry>();

        // Value Elements!
        public StructureExpr RawValue;
        public FilterEntry FormattedValue = FilterEntry.CreateDataEntry("Show");
        // public List<FilterEntryModCommand> FilterEntryModCommands;

        public ExoFilterEntry LookUp(string key)
        {
            if (this.Registry.ContainsKey(key))
            {
                return this.Registry[key];
            }

            return this.GetParent().LookUp(key);
        }

        public ExoFilterEntry GetParent()
        {
            if (this.Type == ExoFilterType.root)
            {
                LoggingFacade.LogWarning("Attempting to get parent of root!");
                throw new Exception("Attempting to get parent of root!");
            }

            return this.Parent;
        }

        public FilterEntry CollectEntryDataFromParent(FilterEntry info)
        {
            throw new NotImplementedException();
        }

        public FilterEntry CollectEntryDataFromChildren(FilterEntry info)
        {
            throw new NotImplementedException();
        }

        public FilterEntry CollectDataToEntry(FilterEntry info) // extractiontype?
        {
            // If we're in an explicit structure. Go up (?):
            // Check properties, return properties in order
            // If we're in an implicit structure. Apply results.
            throw new NotImplementedException();
            this.GetImplicitData(info);
        }

        public FilterEntry GetImplicitData(FilterEntry info)
        {
            foreach (var child in this.Entries)
            {
                throw new NotImplementedException();
                
            }

            return null;
        }
    }
}
