using FilterExo.Core.Parsing;
using FilterExo.Core.PreProcess;
using FilterExo.Core.Process;
using FilterExo.Core.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using FilterExo.Core.Process.GlobalFunctions;
using FilterExo.Model;

namespace FilterExo
{
    public enum ExoBundleType
    {
        filter,
        style
    }

    public class FilterExoFacade
    {
        public bool Debug = true;

        private FilterExoFacade()
        {

        }

        public List<string> RawMetaFilterText = new List<string>();

        private static FilterExoFacade instance;

        public static FilterExoFacade GetInstance()
        {
            if (instance == null)
            {
                instance = new FilterExoFacade();
            }

            return instance;
        }

        public static void InitializeGlobalFunctions()
        {
            // ExoBlock.GlobalFunctions = new Dictionary<string, ExoAtom>();
            // TODO: everything
        }

        public ExoBundle CreateBundle()
        {
            return new ExoBundle();
        }

        public Dictionary<string,string> Execute()
        {
            var results = new Dictionary<string, string>();

            // tokenize
            var tokenizer = new ExoTokenizer();
            tokenizer.Execute(this.RawMetaFilterText);

            // load style information

            // todo

            // 1) BUILD STRUCTURE FROM TOKENIZED EXPRESSION
            var structurizer = new Structurizer();
            var expressionTree = structurizer.Execute(tokenizer.Results);

            if (Debug)
            {
                var structureDebugger = new StructurizerDebugger();
                var treeDict = structureDebugger.SelectOnTree(expressionTree, x => x.Mode);
                var treeDictString = string.Join(System.Environment.NewLine, treeDict.Select(x => $"{x.Key}, {x.Value}"));
                results.Add("structurizer_debug_view", treeDictString);
                results.Add("structurizer_debug_nodenames", structureDebugger.CreateTreeString(expressionTree));
            }

            // 1.5) BUILD -LOGICAL- CONCRETE STRUCTURE

            // 2) RESOLVE DEPEDENCIES

            var exoPreProc = new ExoPreProcessor();
            var exoFilter = exoPreProc.Execute(expressionTree);

            results.Add("ExoPreProcessor", string.Join(System.Environment.NewLine, new ExoPreProcessorDebugger().Execute(exoFilter)));

            // 3) COMPILE INTO SEEDFILTER

            var exoProcessor = new ExoFilterProcessor();

            var seedFilter = exoProcessor.Execute(exoFilter, new ExoStyleDictionary());
            var serializedSeedFilter = seedFilter.SelectMany(x => x.Serialize()).ToList();

            results.Add("ExoOutput", string.Join(System.Environment.NewLine, serializedSeedFilter));

            return results;
        }
    }
}
