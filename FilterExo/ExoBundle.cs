using System.Collections.Generic;
using System.Linq;
using FilterCore.Entry;
using FilterExo.Core.Parsing;
using FilterExo.Core.PreProcess;
using FilterExo.Core.Process;
using FilterExo.Core.Structure;
using FilterExo.Model;

namespace FilterExo
{
    public class ExoBundle
    {
        public bool Debug = true;
        public Dictionary<string, string> DebugData = new Dictionary<string, string>();

        // Tokenization Area
        public List<string> RawInput;
        public ExoTokenizer Tokenizer;
        public List<List<ExoToken>> TokenizerResults;

        // Structurization Area
        public Structurizer Structurizer;
        public StructureExpr StructurizerResults;

        // PreProcessing Area
        public ExoFilter ExoFilter;

        public ExoPreProcessor PreProcessor;
        public ExoProcessor Processor;
        public ExoBundleType Type;

        public ExoBundle SetInput(List<string> rawInput)
        {
            this.RawInput = rawInput;
            return this;
        }

        public ExoBundle SetType(ExoBundleType type)
        {
            this.Type = type;
            return this;
        }

        public ExoBundle AddDependency(ExoBundle dependency)
        {
            return this;
        }

        public ExoBundle Tokenize()
        {
            this.Tokenizer = new ExoTokenizer();
            this.Tokenizer.Execute(this.RawInput);
            this.TokenizerResults = this.Tokenizer.Results;
            return this;
        }

        public ExoBundle Structurize()
        {
            this.Structurizer = new Structurizer();
            this.StructurizerResults = this.Structurizer.Execute(this.Tokenizer.Results);


            if (Debug)
            {
                var structureDebugger = new StructurizerDebugger();
                var treeDict = structureDebugger.SelectOnTree(StructurizerResults, x => x.Mode);
                var treeDictString = string.Join(System.Environment.NewLine, treeDict.Select(x => $"{x.Key}, {x.Value}"));
                this.DebugData.Add("structurizer_debug_view", treeDictString);
                DebugData.Add("structurizer_debug_nodenames", structureDebugger.CreateTreeString(StructurizerResults));
            }

            return this;
        }

        public ExoBundle PreProcess()
        {
            this.PreProcessor = new ExoPreProcessor();
            ExoFilter = PreProcessor.Execute(this.StructurizerResults);
            this.DebugData.Add("ExoPreProcessor", string.Join(System.Environment.NewLine, new ExoPreProcessorDebugger().Execute(ExoFilter)));
            return this;
        }

        public List<FilterEntry> Process()
        {
            this.Processor = new ExoProcessor();
            var serializedResults = Processor.Execute(this.ExoFilter);
            var serializedSeedFilter = serializedResults.SelectMany(x => x.Serialize()).ToList();
            this.DebugData.Add("ExoOutput", string.Join(System.Environment.NewLine, serializedSeedFilter));

            return serializedResults;
        }
    }
}