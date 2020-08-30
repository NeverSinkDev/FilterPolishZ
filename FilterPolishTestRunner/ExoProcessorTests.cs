using FilterCore.Entry;
using FilterExo.Core.Parsing;
using FilterExo.Core.PreProcess;
using FilterExo.Core.PreProcess.Commands;
using FilterExo.Core.Process;
using FilterExo.Core.Structure;
using FilterExo.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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
                "Rule leveledex { ItemLevel >= 81; BaseType \"Exalted Orb\"; };",
                "Rule T1 { BaseType<>; };",
                "Rule T2 { BaseType<>; };",
                "Rule T3 { BaseType<>; };",
                "Rule T4 { BaseType \"Exalted Orb\"; };",
                "# Rule error;",
                "}",
                "Rule T1 { BaseType \"Scroll of Wisdom\"; };"
            };

            var res = this.StringToExoFilter(input);

            Assert.IsNotNull(res);
            Assert.AreEqual(2, res.RootEntry.Scopes.Count);
            Assert.AreEqual(5, res.RootEntry.Scopes[0].Scopes.Count);
            Assert.AreEqual( res.RootEntry.Scopes[0].Scopes[0].Commands[0].Values[0].GetRawValue(), "ItemLevel");
            Assert.AreEqual(res.RootEntry.Scopes[0].Scopes[0].Commands[0].SerializeDebug(), "ItemLevel >= 81");
            Assert.AreEqual(res.RootEntry.Scopes[0].Scopes[0].Commands[1].SerializeDebug(), "BaseType \"Exalted Orb\"");
            Assert.AreEqual(res.RootEntry.Scopes[0].Scopes[4].Commands[0].SerializeDebug(), "BaseType \"Exalted Orb\"");
            Assert.AreEqual(res.RootEntry.Scopes[1].Commands[0].SerializeDebug(), "BaseType \"Scroll of Wisdom\"");
        }

        [Test]
        public void ExoProcessor_BasicExpressionMerging()
        {
            var input = new List<string>()
            {
                "Rule T1 { BaseType ( \"Mirror\" + \"Wisdom\" ); };"
            };

            var res = this.StringToExoFilter(input);

            Assert.AreEqual("BaseType \"Mirror\" \"Wisdom\"", res.RootEntry.Scopes[0].Commands[0].SerializeDebug());
        }

        [Test]
        public void ExoProcessor_VariableExpressionMerging()
        {
            var input = @"var a = ""wisdom"" ""fishing rod"" ""portal"";
                var b = ""mirror"" ""ex"";
                var c = ""zero"";
                Rule T1 { BaseType ( a + b + c ); };";

            var res = this.StringToExoFilter(input.Split(System.Environment.NewLine).ToList());

            Assert.AreEqual(@"BaseType ""ex"" ""fishing rod"" ""mirror"" ""portal"" ""wisdom"" ""zero""", 
                res.RootEntry.Scopes[0].Commands[0].SerializeDebug());
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
        public void ExoPreProcessor_SimpleVariableTests()
        {
            var input = @"# [4112] Incubator

                var t1inc = ""ALPHA"" ""BETA"";
                # var t1inc = ""Geomancer's Incubator"" ""Thaumaturge's Incubator"" ""Time-Lost Incubator"" 

                Rule ONE { BaseType t1inc; };

                Section Incubators : IncubatorBase
                {
	                var t1inc = ""GAMMA"";
	                Rule TWO { BaseType t1inc; };

	                Rule THREE { ItemLevel >= 81; BaseType t1inc; };
	                # Rule error;

	                Rule FOUR { BaseType t1inc; };
                }";

            // var res = this.StringToFilterEntries(input.Split(System.Environment.NewLine).ToList());

            var res = this.StringToExoFilter(input.Split(System.Environment.NewLine).ToList());

            Assert.IsNotNull(res);
            Assert.AreEqual(2, res.RootEntry.Scopes.Count);

            // OUTER SCOPE
            Assert.AreEqual("\"ALPHA\" \"BETA\"", res.RootEntry.Variables["t1inc"].Serialize(res.RootEntry));

            Assert.AreEqual(res.RootEntry.Scopes[0].Commands[0].Values[0].GetRawValue(), "BaseType");
            Assert.AreEqual(res.RootEntry.Scopes[0].Commands[0].SerializeDebug(), "BaseType \"ALPHA\" \"BETA\"");

            // INNER SCOPE
            Assert.AreEqual("\"GAMMA\"", res.RootEntry.Scopes[1].Variables["t1inc"].Serialize(res.RootEntry.Scopes[1]));

            Assert.AreEqual(3, res.RootEntry.Scopes[1].Scopes.Count);
            Assert.AreEqual("BaseType \"GAMMA\"", res.RootEntry.Scopes[1].Scopes[0].Commands[0].SerializeDebug());
            Assert.AreEqual("BaseType \"GAMMA\"", res.RootEntry.Scopes[1].Scopes[0].Commands[0].SerializeDebug());
            Assert.AreEqual("ItemLevel >= 81", res.RootEntry.Scopes[1].Scopes[1].Commands[0].SerializeDebug());
            Assert.AreEqual("BaseType \"GAMMA\"", res.RootEntry.Scopes[1].Scopes[1].Commands[1].SerializeDebug());
            Assert.AreEqual("BaseType \"GAMMA\"", res.RootEntry.Scopes[1].Scopes[2].Commands[0].SerializeDebug());
        }
    }
}
