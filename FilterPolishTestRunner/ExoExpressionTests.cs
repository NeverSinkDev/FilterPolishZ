using FilterExo.Core.Parsing;
using FilterExo.Core.PreProcess;
using FilterExo.Core.PreProcess.Commands;
using FilterExo.Core.Process;
using FilterExo.Core.Structure;
using FilterExo.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilterPolishTestRunner
{
    [TestFixture]
    public class ExoExpressionTests
    {
        private Structurizer Structurizer;
        private ExoTokenizer Tokenizer;
        private ExoPreProcessor PreProcessor;
        private ExoProcessor Processor;

        [SetUp]
        public void Prepare()
        {
            this.Structurizer = new Structurizer();
            this.Tokenizer = new ExoTokenizer();
            this.PreProcessor = new ExoPreProcessor();
            this.Processor = new ExoProcessor();
        }

        public ExoFilter StringToExoFilter(List<string> input)
        {
            Tokenizer.Execute(input);
            var structurizerOutput = Structurizer.Execute(Tokenizer.Results);
            var preproc = PreProcessor.Execute(structurizerOutput);
            return preproc;
        }

        [Test]
        public void ExoExpression_TreeParsing()
        {
            string s = "Show Wat { a(1,1) + 2 + b((3+3),(4+4)) };";

            var res = this.StringToExoFilter(new List<string>() { s });
         
            Assert.IsNotNull(res);

            var command = (res.RootEntry.Scopes[0].Commands[0] as ExoExpressionCommand);
            Assert.IsNotNull(command);

            var tree = command.CreateIndentationTree(command.Values);
            Assert.IsNotNull(tree);
        }
    }
}
