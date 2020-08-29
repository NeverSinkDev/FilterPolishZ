using FilterCore;
using FilterCore.Entry;
using FilterCore.Line;
using FilterExo.Core.PreProcess.Commands;
using FilterExo.Core.Structure;
using FilterPolishUtil;
using FilterPolishUtil.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static FilterExo.FilterExoConfig;
using static FilterPolishUtil.TraceUtility;

namespace FilterExo.Model
{
    public class ExoBlock
    {
        public ExoFilterType Type;
        public string Name;

        // Hierarchical elements
        public ExoBlock Parent;
        public List<ExoBlock> Scopes = new List<ExoBlock>();

        public Dictionary<string, IExoVariable> Variables { get; set; } = new Dictionary<string, IExoVariable>();
        public Dictionary<string, ExoFunction> Functions { get; set; } = new Dictionary<string, ExoFunction>();
        
        public List<IExoCommand> Commands { get; set; } = new List<IExoCommand>();

        public FilterEntry ResolveAndSerialize()
        {
            var entry = FilterEntry.CreateDataEntry("Show");

            ResolveVariables();
            ResolveStyleFile();

            foreach (var comm in this.Commands)
            {
                switch (comm.Type)
                {
                    case IExoCommandType.scope:
                        break;
                    case IExoCommandType.execution:
                        break;
                    case IExoCommandType.filter:
                        entry.Content.Add((comm as ExoExpressionCommand).Serialize());
                        break;
                    default:
                        break;
                }
            }

            void ResolveVariables()
            {

            }

            void ResolveStyleFile()
            {

            }

            return entry;
        }

        public void StoreVariable(string name, List<string> variableContent)
        {
            Check(FilterCore.FilterGenerationConfig.TierTagSort.ContainsKey(name), "variable uses reserved name!");
            Check(FilterCore.FilterGenerationConfig.LineTypesSort.ContainsKey(name), "variable uses reserved name!");
            Check(FilterCore.FilterGenerationConfig.ValidRarities.Contains(name), "variable uses reserved name!");
            Check(name.ContainsSpecialCharacters(), "variable uses invalid characters!");

            this.Variables.Add(name, new SimpleExoVariable(variableContent));
        }

        public IExoVariable GetVariable(string key)
        {
            return GetInternalVariable(key);
        }

        public bool IsVariable(string key)
        {
            if (this.Variables.ContainsKey(key))
            {
                return true;
            }

            if (this.Type == ExoFilterType.root)
            {
                return false;
            }

            return this.GetParent().IsVariable(key);
        }

        private IExoVariable GetInternalVariable(string key)
        {
            if (this.Variables.ContainsKey(key))
            {
                return this.Variables[key];
            }

            return this.GetParent().GetVariable(key);
        }

        public ExoBlock GetParent()
        {
            if (this.Type == ExoFilterType.root)
            {
                LoggingFacade.LogWarning("Attempting to get parent of root!");
                throw new Exception("Attempting to get parent of root!");
            }

            return this.Parent;
        }

        public void AddCommand(IExoCommand command)
        {
            this.Commands.Add(command);
            command.SetParent(this);
        }

        public List<string> Debug_GetSummary()
        {
            var results = new List<string>();
            results.Add($"TYPE: {this.Type.ToString()} // CHILDREN: {this.Scopes.Count}");
            results.Add($"VARIABLES: {string.Join(" ", this.Variables.Keys)}");
            results.AddRange(this.Commands.Select(x => x.SerializeDebug()));
            return results;
        }
    }
}
