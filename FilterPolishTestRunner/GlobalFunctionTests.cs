using System;
using System.Collections.Generic;
using System.Text;
using FilterCore.Entry;
using FilterExo.Core.Parsing;
using FilterExo.Core.PreProcess;
using FilterExo.Core.Process;
using FilterExo.Core.Structure;
using FilterExo.Model;
using NUnit.Framework;

namespace FilterPolishTestRunner
{
    [TestFixture]
    public class GlobalFunctionTests
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

        public List<FilterEntry> StringToFilterEntries(List<string> input)
        {
            Tokenizer.Execute(input);
            var structurizerOutput = Structurizer.Execute(Tokenizer.Results);
            var preproc = PreProcessor.Execute(structurizerOutput);
            var proc = Processor.Execute(preproc);
            return proc;
        }

        public ExoFilter StringToExoFilter(List<string> input)
        {
            Tokenizer.Execute(input);
            var structurizerOutput = Structurizer.Execute(Tokenizer.Results);
            var preproc = PreProcessor.Execute(structurizerOutput);
            return preproc;
        }

        [Test]
        public void TypeTagFunctionTest()
        {
            var input = new List<string>()
            {
                "Section Incubators",
                "{",
                    "Show T1 { BaseType \"Exalted Orb\"; Type(); };",
                "}",
            };

            var res = this.StringToFilterEntries(input);
            Assert.IsNotNull(res);
            Assert.AreEqual(1, res.Count);

            // worst test ever
            Assert.IsTrue(res[0].SerializeMergedString.Contains("%HS5"));
        }
    }
}
