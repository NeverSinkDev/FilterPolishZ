using FilterExo.Core.Parsing;
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

        public void Execute()
        {
            // tokenize
            var tokenizer = new ExoTokenizer();
            tokenizer.Execute(this.RawMetaFilterText);

            // build structure - detect expression, build tree
            // load style information
            // evaluate expressions.
        }
    }
}
