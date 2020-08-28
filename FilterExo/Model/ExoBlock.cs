using FilterCore;
using FilterCore.Entry;
using FilterCore.Line;
using FilterExo.Core.PreProcess.Commands;
using FilterExo.Core.Structure;
using FilterPolishUtil.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static FilterExo.FilterExoConfig;

namespace FilterExo.Model
{
    public class ExoBlock
    {
        public ExoFilterType Type;

        // Hierarchical elements
        public ExoBlock Parent;
        public List<ExoBlock> Scopes = new List<ExoBlock>();

        public Dictionary<string, IExoVariable> Variables { get; set; } = new Dictionary<string, IExoVariable>();
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
                        entry.Content.Add((comm as ExoFilterLineCommand).Serialize());
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

            results.Add($"TYPE: {this.Type.ToString()}");
            results.AddRange(this.Commands.Select(x => x.SerializeDebug()));
            return results;
        }
    }
}
