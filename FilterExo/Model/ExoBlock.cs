using FilterCore;
using FilterCore.Entry;
using FilterCore.Line;
using FilterExo.Core.PreProcess.Commands;
using FilterExo.Core.Structure;
using FilterPolishUtil;
using FilterPolishUtil.Extensions;
using FilterPolishUtil.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static FilterExo.FilterExoConfig;
using static FilterPolishUtil.TraceUtility;

namespace FilterExo.Model
{
    [DebuggerDisplay("{debugView}")]
    public class ExoBlock
    {
        private string debugView => $"{this.Type.ToString()} C:{this.Scopes.Count} DATA:{this.Mutators.Count + this.Commands.Count} VF:{this.Variables.Count + this.Functions.Count} #:{this.SimpleComments.Count}";

        public ExoFilterType Type = ExoFilterType.generic;

        // Hierarchical elements
        public ExoBlock Parent;
        public List<ExoBlock> Scopes = new List<ExoBlock>();

        // functions, properties, interfaces that get applied to all children.
        public List<ExoExpressionCommand> Mutators { get; set; } = new List<ExoExpressionCommand>();

        public Dictionary<string, ExoAtom> Variables { get; set; } = new Dictionary<string, ExoAtom>();
        public Dictionary<string, ExoAtom> Functions { get; set; } = new Dictionary<string, ExoAtom>();
        public List<ExoExpressionCommand> Commands { get; set; } = new List<ExoExpressionCommand>();

        public List<string> SimpleComments = new List<string>();

        private List<ExoExpressionCommand> TemporaryCommandStorage { get; set; } = new List<ExoExpressionCommand>();

        public IEnumerable<List<string>> ResolveAndSerialize()
        {
            if (this.Type == ExoFilterType.comment)
            {
                return this.SimpleComments.Select(x => new List<string>() { x });
            }
            else
            {
                var mutatorCommands = ResolveAndSerializeSingleSection(ExoExpressionCommandSource.mutator).ToList();
                var directCommands = ResolveAndSerializeSingleSection(ExoExpressionCommandSource.direct).ToList();

                return EIEnumerable.YieldTogether(mutatorCommands, directCommands);
            }

        }

        public IEnumerable<List<string>> ResolveAndSerializeSingleSection(ExoExpressionCommandSource target)
        {
            this.InitializeTemporaryStorage(target);

            var count = TemporaryCommandStorage.Count;
            for (int i = 0; i < count; i++)
            {
                ExoExpressionCommand comm = TemporaryCommandStorage[i];
                comm.ExecutionContext = i;
                comm.Exectutor = this;

                if (comm.ContainerCommand)
                {
                    count = TemporaryCommandStorage.Count;
                    continue;
                }

                var result = comm.Serialize();

                if (comm.ContainerCommand)
                {
                    count = TemporaryCommandStorage.Count;
                    continue;
                }

                if (result.Count == 0)
                {
                    continue;
                }

                yield return result;
            }
        }

        public void StoreVariable(string name, List<string> variableContent)
        {
            Check(FilterCore.FilterGenerationConfig.TierTagSort.ContainsKey(name), "variable uses reserved name!");
            Check(FilterCore.FilterGenerationConfig.LineTypesSort.ContainsKey(name), "variable uses reserved name!");
            Check(FilterCore.FilterGenerationConfig.ValidRarities.Contains(name), "variable uses reserved name!");
            Check(name.ContainsSpecialCharacters(), "variable uses invalid characters!");

            // we treat the variable as a command/expression to allow internal simplifications
            var command = new ExoExpressionCommand(variableContent);
            command.SetParent(this);
            var content = command.ResolveExpression();

            // since we don't know if there's 1 or many variables after the simplifications, we store result as a "pack"
            this.Variables.Add(name, new ExoAtom(content));
        }

        public ExoAtom GetVariable(string key)
        {
            return GetInternalVariable(key);
        }

        public ExoAtom GetFunction(string key)
        {
            return GetInternalFunction(key);
        }

        public IEnumerable<ExoExpressionCommand> YieldMutators()
        {
            if (this.Type != ExoFilterType.root)
            {
                var parentMutators = this.GetParent().YieldMutators();
                foreach (var item in parentMutators)
                {
                    yield return item;
                }
            }

            foreach (var item in this.Mutators)
            {
                yield return item;
            }
        }

        internal bool IsFunction(string key)
        {
            if (this.Functions.ContainsKey(key))
            {
                return true;
            }

            if (this.Type == ExoFilterType.root)
            {
                return false;
            }

            return this.GetParent().IsFunction(key);
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

        private ExoAtom GetInternalVariable(string key)
        {
            if (this.Variables.ContainsKey(key))
            {
                return this.Variables[key];
            }

            return this.GetParent().GetVariable(key);
        }

        private ExoAtom GetInternalFunction(string key)
        {
            if (this.Functions.ContainsKey(key))
            {
                return this.Functions[key];
            }

            return this.GetParent().GetFunction(key);
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

        public void AddCommand(ExoExpressionCommand command)
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

        public void InsertCommands(List<List<ExoAtom>> funcRes, ExoExpressionCommand exoExpressionCommand)
        {
            var context = exoExpressionCommand.ExecutionContext + 1;
            TemporaryCommandStorage.InsertRange(context,
                    funcRes.Select(x => new ExoExpressionCommand(x)
                    {
                        DerivedCommand = true,
                        Parent = this,
                        Source = exoExpressionCommand.Source
                    }));
        }

        private void InitializeTemporaryStorage(ExoExpressionCommandSource sourceType)
        {
            this.TemporaryCommandStorage = new List<ExoExpressionCommand>();
            switch (sourceType)
            {
                case ExoExpressionCommandSource.direct:
                    foreach (var item in this.Commands)
                    {
                        this.TemporaryCommandStorage.Add(item);
                    }
                    break;
                case ExoExpressionCommandSource.mutator:
                    foreach (var item in this.YieldMutators())
                    {
                        this.TemporaryCommandStorage.Add(item);
                    }
                    break;
                case ExoExpressionCommandSource.style:
                    break;
            }
        }

        private IEnumerable<ExoExpressionCommand> GetCommandSource(ExoExpressionCommandSource sourceType)
        {
            switch (sourceType)
            {
                case ExoExpressionCommandSource.direct:
                    return this.Commands;
                case ExoExpressionCommandSource.mutator:
                    return this.Mutators;
                case ExoExpressionCommandSource.style:
                    return this.Commands;
                default:
                    throw new Exception("unknown command source!");
            }
        }
    }
}