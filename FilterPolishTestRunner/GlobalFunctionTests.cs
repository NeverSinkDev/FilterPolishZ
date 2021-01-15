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
        private ExoFilterProcessor _filterProcessor;

        [SetUp]
        public void Prepare()
        {
            this.Structurizer = new Structurizer();
            this.Tokenizer = new ExoTokenizer();
            this.PreProcessor = new ExoPreProcessor();
            this._filterProcessor = new ExoFilterProcessor();
            this._filterProcessor.AddEmptyLines = false;
        }

        public List<FilterEntry> StringToFilterEntries(List<string> input)
        {
            Tokenizer.Execute(input);
            var structurizerOutput = Structurizer.Execute(Tokenizer.Results);
            var preproc = PreProcessor.Execute(structurizerOutput);
            var proc = _filterProcessor.Execute(preproc, new ExoStyleDictionary());
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
                "Func IncubatorBase(){ Class \"Currency\"; Tierlist(\"currency->incubators\"); }",
                "Section Incubators : IncubatorBase",
                "{",
                    "Show T1 { BaseType \"Obscure Incubator\"; Tier(\"T1\"); };",
                "}",
            };
            var res = this.StringToFilterEntries(input);
            Assert.IsNotNull(res);
            Assert.AreEqual(1, res.Count);

            // worst test ever
            Assert.IsTrue(res[0].SerializeMergedString.Contains("$type->currency->incubators"));
            Assert.IsTrue(res[0].SerializeMergedString.Contains("$tier->t1"));
        }
    }
}
