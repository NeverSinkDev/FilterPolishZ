using FilterCore.Entry;
using FilterExo.Core.Parsing;
using FilterExo.Core.PreProcess;
using FilterExo.Core.Process;
using FilterExo.Core.Structure;
using FilterExo.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace FilterPolishTestRunner
{
    [TestFixture]
    public class ExoProcessorTests
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
        public void ExoProcessor_BasicTests1()
        {
            var input = new List<string>()
            {
                "Section Incubators : IncubatorBase",
                "{",
                "Rule leveledex { ItemLevel >= 81; BaseType<>; };",
                "Rule T1 { BaseType<>; };",
                "Rule T2 { BaseType<>; };",
                "Rule T3 { BaseType<>; };",
                "Rule T4 { BaseType<>; };",
                "# Rule error;",
                "}"
            };

            var res = this.StringToExoFilter(input);

            Assert.IsNotNull(res);
            Assert.AreEqual(res.RootEntry.Scopes.Count, 5);
            // Assert.AreEqual(res.RootEntry.Scopes[0].Commands.Count, 2);
            // Assert.AreEqual(res.RootEntry.Scopes[0].Commands["ItemLevel"].Command, "ItemLevel");
            // Assert.AreEqual(res.RootEntry.Scopes[0].Commands["ItemLevel"].Value[0][0], ">=");
            // Assert.AreEqual(res.RootEntry.Scopes[0].Commands["ItemLevel"].Value[0][1], "81");
        }

        [Test]
        public void ExoProcessor_SingleRuleSerialization()
        {
            var input = new List<string>()
            {
                "Rule T1 { BaseType<>; };"
            };

            var res = this.StringToFilterEntries(input);

            Assert.IsTrue(res.Count == 1);
            Assert.IsTrue(res[0].SerializeMergedString.Contains("Show"));
            Assert.IsTrue(res[0].SerializeMergedString.Contains("BaseType <>"));
        }

        [Test]
        public void ExoPreProcessor_TestVar()
        {
            var input = new List<string>()
            {
               @"var t1inc = ""Geomancer's Incubator"" ""Thaumaturge's Incubator"" ""Time-Lost Incubator"" ",
                "Section Incubators : IncubatorBase",
                "{",
                "Rule T1 { ItemLevel >= 81; BaseType $t1inc; };",
                "Rule T2 { BaseType $t1inc; };",
                "# Rule error;",
                "}",
                "Rule T3 { BaseType $t1inc; };",
            };

            var res = this.StringToFilterEntries(input);

            Assert.IsNotNull(res);

            //Assert.IsNotNull(res);
            //Assert.AreEqual(res.RootEntry.Scopes.Count,5);
            //Assert.AreEqual(res.RootEntry.Scopes[0].FormattedValue.Content.Content.Count,2);
            //Assert.AreEqual(res.RootEntry.Scopes[1].FormattedValue.Content.Content.Count,1);
            //Assert.IsTrue(res.RootEntry.Scopes[0].FormattedValue.Content.Content.ContainsKey("ItemLevel"));
            //Assert.IsTrue(res.RootEntry.Scopes[0].FormattedValue.Content.Content.ContainsKey("BaseType"));
            //Assert.IsTrue(res.RootEntry.Scopes[0].FormattedValue.Content.Content["ItemLevel"][0].Serialize() == "ItemLevel >= 81");
            //Assert.AreEqual(res.RootEntry.Scopes[1].FormattedValue.Content.Content.Count,1);
        }
    }
}
