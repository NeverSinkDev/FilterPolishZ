﻿using FilterCore.Entry;
using FilterExo.Core.Parsing;
using FilterExo.Core.PreProcess;
using FilterExo.Core.PreProcess.Commands;
using FilterExo.Core.Process;
using FilterExo.Core.Structure;
using FilterExo.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                "#------------------------------------",
                "# [0702] Layer - T2 - ECONOMY",
                "#------------------------------------",
                "",
                "Section Incubators",
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
            Assert.AreEqual(3, res.RootEntry.Scopes.Count);
            Assert.AreEqual(6, res.RootEntry.Scopes[1].Scopes.Count);
            Assert.AreEqual(3, res.RootEntry.Scopes[0].SimpleComments.Count);
            Assert.AreEqual("# [0702] Layer - T2 - ECONOMY", res.RootEntry.Scopes[0].SimpleComments[1]);
            Assert.AreEqual("leveledex", res.RootEntry.Scopes[1].Scopes[0].Name);
            Assert.AreEqual("ItemLevel >= 81", res.RootEntry.Scopes[1].Scopes[0].Commands[0].SerializeDebug());
            Assert.AreEqual("ItemLevel", res.RootEntry.Scopes[1].Scopes[0].Commands[0].Values[0].GetRawValue());
            Assert.AreEqual("BaseType \"Exalted Orb\"", res.RootEntry.Scopes[1].Scopes[0].Commands[1].SerializeDebug());
            Assert.AreEqual("BaseType \"Exalted Orb\"", res.RootEntry.Scopes[1].Scopes[4].Commands[0].SerializeDebug());
            Assert.AreEqual("BaseType \"Scroll of Wisdom\"", res.RootEntry.Scopes[2].Commands[0].SerializeDebug());
        }

        [Test]
        public void ExoProcessor_Stacking()
        {
            var input = new List<string>()
            {
                "Func CurrencyBase(){ SetTextColor 200 0 0 255; BG 255 255 255 255; }",
                "Func IncubatorBase(){ SetTextColor 255 0 0 255; }",
                "Section Currency : CurrencyBase",
                "{",
                    "Section Incubators :  IncubatorBase",
                    "{",
                        "Rule leveledex { BaseType \"Obscure Orb\"; };",
                    "}",
                "}",
            };

            var res = this.StringToExoFilter(input);

            Assert.IsNotNull(res);

            var commands0 = res.RootEntry.Scopes[0].Scopes[0].Scopes[0].ResolveAndSerialize().ToList();

            Assert.AreEqual("SetTextColor 200 0 0 255", string.Join(" ", commands0[0]));
            Assert.AreEqual("SetBackgroundColor 255 255 255 255", string.Join(" ", commands0[1]));
            Assert.AreEqual("SetTextColor 255 0 0 255", string.Join(" ", commands0[2]));
            Assert.AreEqual("BaseType \"Obscure Orb\"", string.Join(" ", commands0[3]));
        }

        [Test]
        public void ExoProcessor_BasicMutatorTest()
        {
            var input = new List<string>()
            {
                "Func CurrencyBase(){ SetTextColor 200 0 0 255; BG 255 255 255 255; }",
                "Func IncubatorBase(){ SetTextColor 255 0 0 255; }",
                "Section Incubators : CurrencyBase, IncubatorBase",
                "{",
                "Rule leveledex { ItemLevel >= 81; BaseType \"Exalted Orb\"; };",
                "Rule T4 { BaseType \"Exalted Orb\"; };",
                "# Rule error;",
                "}",
                "Rule T1 { BaseType \"Scroll of Wisdom\"; };"
            };

            var res = this.StringToExoFilter(input);

            Assert.IsNotNull(res);
            Assert.AreEqual(2, res.RootEntry.Scopes.Count);
            Assert.AreEqual(2, res.RootEntry.Scopes[0].Mutators.Count);
            Assert.AreEqual(3, res.RootEntry.Scopes[0].Scopes.Count);

            var commands0 = res.RootEntry.Scopes[0].Scopes[0].ResolveAndSerialize().ToList();

            Assert.AreEqual("SetTextColor 200 0 0 255", string.Join(" ", commands0[0]));
            Assert.AreEqual("SetBackgroundColor 255 255 255 255", string.Join(" ", commands0[1]));
            Assert.AreEqual("SetTextColor 255 0 0 255", string.Join(" ", commands0[2]));
            Assert.AreEqual("ItemLevel >= 81", string.Join(" ", commands0[3]));

            Assert.AreEqual("BaseType \"Scroll of Wisdom\"", res.RootEntry.Scopes[1].Commands[0].SerializeDebug());
        }

        [Test]
        public void ExoProcessor_BasicFunctions()
        {
            var input = new List<string>()
            {
                "Func test (a,b) { SetBorderColor a 0 0 b; SetTextColor a 0 0 b; };",
                "Section Incubators",
                "{",
                "Rule leveledex { ItemLevel >= 81; BaseType \"Exalted Orb\"; test(100,200); };",
                "Rule T1 { test(100,200); };",
                "# Rule error;",
                "}"
            };

            // repeated execution
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            for (int i = 0; i < 3; i++)
            {
                var res = this.StringToFilterEntries(input).Select(x => x.Serialize()).ToList();
                Assert.IsNotNull(res);
                Assert.AreEqual(3, res.Count);
                Assert.AreEqual("\tSetTextColor 100 0 0 200", res[0][3]);
                Assert.AreEqual("\tSetBorderColor 100 0 0 200", res[0][4]);
            }
            sw.Stop();
            Debug.WriteLine(sw.Elapsed);
        }

        [Test]
        public void ExoProcessor_LayeredFunctions()
        {
            var input = new List<string>()
            {
                "Func xVal () { 2 };",
                "Func yVal (a) { a };",
                "Func test (a) { BD 1 xVal() 3 4; TX 1 xVal() yVal(a) 4; };",
                "Section Incubators",
                "{",
                "Rule leveledex { ItemLevel >= 81; BaseType \"Exalted Orb\"; test(3); };",
                "Rule T1 { test(3); };",
                "# Rule error;",
                "}"
            };

            var res = this.StringToFilterEntries(input).Select(x => x.Serialize()).ToList();

            Assert.IsNotNull(res);
            Assert.AreEqual(3, res.Count);
            Assert.AreEqual("\tSetTextColor 1 2 3 4", res[0][3]);
            Assert.AreEqual("\tSetBorderColor 1 2 3 4", res[0][4]);
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
                var b = ""mirror"" ""ex"" - ""ex"";
                var c = ""zero"" ""sword"";
                var d = ""wisdom"";
                Rule T1 { BaseType ( a + b + c - d ); };";

            var res = this.StringToExoFilter(input.Split(System.Environment.NewLine).ToList());

            Assert.AreEqual(@"BaseType ""fishing rod"" ""mirror"" ""portal"" ""sword"" ""zero""", 
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

            // repeated execution
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            for (int i = 0; i < 3; i++)
            {
                var res = this.StringToExoFilter(input.Split(System.Environment.NewLine).ToList());

                Assert.IsNotNull(res);
                Assert.AreEqual(3, res.RootEntry.Scopes.Count);

                // OUTER SCOPE
                Assert.AreEqual("\"ALPHA\" \"BETA\"", res.RootEntry.Variables["t1inc"].Serialize(res.RootEntry));

                Assert.AreEqual("BaseType", res.RootEntry.Scopes[1].Commands[0].Values[0].GetRawValue());
                Assert.AreEqual("BaseType \"ALPHA\" \"BETA\"", res.RootEntry.Scopes[1].Commands[0].SerializeDebug());

                // INNER SCOPE
                Assert.AreEqual("\"GAMMA\"", res.RootEntry.Scopes[2].Variables["t1inc"].Serialize(res.RootEntry.Scopes[1]));

                Assert.AreEqual(4, res.RootEntry.Scopes[2].Scopes.Count);
                Assert.AreEqual("BaseType \"GAMMA\"", res.RootEntry.Scopes[2].Scopes[0].Commands[0].SerializeDebug());
                Assert.AreEqual("BaseType \"GAMMA\"", res.RootEntry.Scopes[2].Scopes[0].Commands[0].SerializeDebug());
                Assert.AreEqual("ItemLevel >= 81", res.RootEntry.Scopes[2].Scopes[1].Commands[0].SerializeDebug());
                Assert.AreEqual("BaseType \"GAMMA\"", res.RootEntry.Scopes[2].Scopes[1].Commands[1].SerializeDebug());
                Assert.AreEqual("BaseType \"GAMMA\"", res.RootEntry.Scopes[2].Scopes[3].Commands[0].SerializeDebug());
            }
            sw.Stop();
            Debug.WriteLine(sw.Elapsed);


        }
    }
}
