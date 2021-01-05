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
using FilterExo.Core.Process;
using FilterExo.Core.Process.GlobalFunctions;
using FilterExo.Core.Process.GlobalFunctions.StyleFunctions;
using FilterExo.Core.Process.StyleResoluton;
using static FilterExo.FilterExoConfig;
using static FilterPolishUtil.TraceUtility;

namespace FilterExo.Model
{
    [DebuggerDisplay("{debugView}")]
    public class ExoBlock
    {
        static ExoBlock()
        {
            new AddTimeCommentFunction().Integrate();
            new TypeTagFunction().Integrate();
            new TierTagFunction().Integrate();
            new AutoTierFunction().Integrate();
            new EmptyFunction().Integrate();

            new ApplyStyleFunction().Integrate();
        }

        public string debugView => $"{this.Name} C:{this.Scopes.Count} D:{this.Mutators.Count + this.Commands.Count} VF:{this.Variables.Count + this.Functions.Count} #:{this.SimpleComments.Count} {this.Type.ToString()}";

        // Descriptive properties
        public string Name { get; internal set; }
        public string DescriptorCommand { get; set; }
        public ExoFilterType Type = ExoFilterType.generic;

        public List<ExoBlock> References;

        // Hierarchical elements
        public ExoBlock Parent;
        public List<ExoBlock> Scopes = new List<ExoBlock>();

        // functions, properties, interfaces that get applied to all children.
        public List<ExoExpressionCommand> Mutators { get; set; } = new List<ExoExpressionCommand>();
        public Dictionary<string, ExoAtom> Variables { get; set; } = new Dictionary<string, ExoAtom>();
        public Dictionary<string, ExoAtom> Functions { get; set; } = new Dictionary<string, ExoAtom>();
        public List<ExoExpressionCommand> Commands { get; set; } = new List<ExoExpressionCommand>();

        public List<ExoBlock> LinkedBlocks { get; set; } = new List<ExoBlock>();

        public static Dictionary<string, ExoAtom> GlobalFunctions { get; set; } = new Dictionary<string, ExoAtom>();

        public List<string> SimpleComments = new List<string>();

        // Execution parameters - ugly
        private List<ExoExpressionCommand> TemporaryCommandStorage { get; set; } = new List<ExoExpressionCommand>();

        public IEnumerable<List<string>> ResolveAndSerializeStyle()
        {
            var linkedResults = new List<List<string>>();
            foreach (var linkedBlock in this.LinkedBlocks)
            {
                var linkedBlockResults = linkedBlock.ResolveAndSerializeStyle();
                foreach (var linkedBlockResult in linkedBlockResults)
                {
                    linkedResults.Add(linkedBlockResult);
                }
            }

            var mutatorCommands = ResolveAndSerializeSingleSection(ExoExpressionCommandSource.mutator).ToList();
            var directCommands = ResolveAndSerializeSingleSection(ExoExpressionCommandSource.direct).ToList();
            return EIEnumerable.YieldTogether(linkedResults, mutatorCommands, directCommands);
        }

        public IEnumerable<List<string>> ResolveAndSerialize(ExoStyleDictionary styleDict)
        {
            if (this.Type == ExoFilterType.comment)
            {
                return this.SimpleComments.Select(x => new List<string>() { x }).ToList();
            }
            else
            {
                var linkedResults = new List<List<string>>();
                foreach (var linkedBlock in this.LinkedBlocks)
                {
                    var linkedBlockResults = linkedBlock.ResolveAndSerializeStyle();
                    foreach (var linkedBlockResult in linkedBlockResults)
                    {
                        linkedResults.Add(linkedBlockResult);
                    }
                }

                var mutatorCommands = ResolveAndSerializeSingleSection(ExoExpressionCommandSource.mutator).ToList();
                var directCommands = ResolveAndSerializeSingleSection(ExoExpressionCommandSource.direct).ToList();

                List<List<string>> styleCommands = new List<List<string>>();
                if (styleDict != null)
                {
                    styleCommands = StyleResolutionStrategy.Execute(this, styleDict);
                }

                return EIEnumerable.YieldTogether(linkedResults, mutatorCommands, directCommands, styleCommands);
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
                comm.Executor = this;

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

            name = name.ToLower();

            // we treat the variable as a command/expression to allow internal simplifications
            var command = new ExoExpressionCommand(variableContent);
            command.SetParent(this);
            var content = command.ResolveExpression();

            // since we don't know if there's 1 or many variables after the simplifications, we store result as a "pack"
            this.Variables.Add(name, new ExoAtom(content));
        }

        public ExoAtom GetVariable(string key)
        {
            key = key.ToLower();
            if (this.Variables.ContainsKey(key))
            {
                return this.Variables[key];
            }

            foreach (var block in this.LinkedBlocks)
            {
                if (block.IsVariable(key))
                {
                    return block.GetVariable(key);
                }
            }

            return this.GetParent().GetVariable(key);
        }

        public ExoAtom GetFunction(string key)
        {
            key = key.ToLower();
            if (GlobalFunctions.ContainsKey(key))
            {
                return GlobalFunctions[key];
            }

            if (this.Functions.ContainsKey(key))
            {
                return this.Functions[key];
            }

            foreach (var block in this.LinkedBlocks)
            {
                if (block.IsFunction(key))
                {
                    return block.GetFunction(key);
                }
            }

            return this.GetParent().GetFunction(key);
        }

        public ExoFunction GetFunctionDirect(string key)
        {
            key = key.ToLower();
            if (GlobalFunctions.ContainsKey(key))
            {
                return GlobalFunctions[key].GetFunction(this);
            }

            if (this.Functions.ContainsKey(key))
            {
                return this.Functions[key].GetFunction(this);
            }

            foreach (var block in this.LinkedBlocks)
            {
                if (block.IsFunction(key))
                {
                    return block.GetFunctionDirect(key);
                }
            }

            return this.GetParent().GetFunctionDirect(key);
        }

        public IEnumerable<ExoFunction> YieldLinkedFunctions(string key)
        {
            key = key.ToLower();
            var funcs = new List<ExoFunction>();

            if (GlobalFunctions.ContainsKey(key))
            {
                return new List<ExoFunction>() { GlobalFunctions[key].GetFunction(this)};
            }

            if (this.Functions.ContainsKey(key))
            {
                funcs.Add(this.Functions[key].GetFunction(this));
            }

            if (this.Parent != null)
            {
                foreach (var func in this.Parent.YieldLinkedFunctions(key))
                {
                    funcs.Add(func);
                }
            }

            foreach (var block in this.LinkedBlocks)
            {
                foreach (var func in block.YieldLinkedFunctions(key))
                {
                    funcs.Add(func);
                }
            }

            return funcs.Distinct();
        }

        public IEnumerable<ExoFunction> YieldFunctions(string key)
        {
            var results = new List<ExoFunction>();

            key = key.ToLower();

            if (IsFunction(key))
            {
                results.Add(GetFunctionDirect(key));
            }

            foreach (var block in this.LinkedBlocks)
            {
                if (block.IsFunction(key))
                {
                    results.Add(block.GetFunctionDirect(key));
                }
            }

            return results;
        }

        public IEnumerable<ExoExpressionCommand> YieldMutators()
        {
            foreach (var block in this.LinkedBlocks)
            {
                var mutators = block.YieldMutators();
                foreach (var mutator in mutators)
                {
                    yield return mutator;
                }
            }

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
            key = key.ToLower();
            if (GlobalFunctions.ContainsKey(key))
            {
                return true;
            }

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

        public IEnumerable<ExoBlock> FindChildSection(string key)
        {
            foreach (var exoBlock in this.Scopes)
            {
                if (string.Equals(key,exoBlock.Name, StringComparison.OrdinalIgnoreCase))
                {
                    yield return exoBlock;
                }
                
                var childRes = exoBlock.FindChildSection(key);
                foreach (var res in childRes)
                {
                    yield return res;
                }
            }
        }

        public IEnumerable<ExoBlock> FindChildSectionFromRoot(string key)
        {
            var target = this.GetRoot();

            foreach (var exoBlock in target.Scopes)
            {
                if (string.Equals(key, exoBlock.Name, StringComparison.OrdinalIgnoreCase))
                {
                    yield return exoBlock;
                }

                var childRes = exoBlock.FindChildSection(key);
                foreach (var res in childRes)
                {
                    yield return res;
                }
            }
        }

        public bool IsVariable(string key)
        {
            key = key.ToLower();
            if (this.Variables.ContainsKey(key))
            {
                return true;
            }

            foreach (var linkedBlock in this.LinkedBlocks)
            {
                if (linkedBlock.IsVariable(key))
                {
                    return true;
                }
            }

            if (this.Type == ExoFilterType.root)
            {
                return false;
            }

            return this.GetParent().IsVariable(key);
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

        public ExoBlock GetRoot()
        {
            if (this.Type == ExoFilterType.root)
            {
                return this;
            }
            else
            {
                return this.Parent.GetRoot();
            }
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
            // results.AddRange(this.Commands.Select(x => x.SerializeDebug()));
            return results;
        }

        // RUNTIME FUNCTIONS

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
                        item.ContainerCommand = false;
                    }
                    break;
                case ExoExpressionCommandSource.mutator:
                    foreach (var item in this.YieldMutators())
                    {
                        this.TemporaryCommandStorage.Add(item);
                        item.ContainerCommand = false;
                    }
                    break;
                case ExoExpressionCommandSource.style:
                    break;
            }
        }
    }
}