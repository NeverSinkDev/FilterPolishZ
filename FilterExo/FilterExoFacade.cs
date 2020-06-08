using FilterExo.Core.Parsing;
using FilterExo.Core.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilterExo
{
    public class FilterExoFacade
    {
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

        public string Execute()
        {
            // tokenize
            var tokenizer = new ExoTokenizer();
            tokenizer.Execute(this.RawMetaFilterText);

            // load style information
            var structurizer = new Structurizer();
            var expressionTree = structurizer.Execute(tokenizer.Results);

            var structureDebugger = new StructurizerDebugger();
            return structureDebugger.Execute(expressionTree);

            

            // build structure - detect expression, build tree
            // 1) BUILD STRUCTURE FROM TOKENIZED EXPRESSION
            // 1.5) BUILD -LOGICAL- CONCREtE STRUCTURE
            // 2) RESOLVE DEPEDENCIES



            // 3) COMPILE INTO SEEDFILTER
        }
    }
}
