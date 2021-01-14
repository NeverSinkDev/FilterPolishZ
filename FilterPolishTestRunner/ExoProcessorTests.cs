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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
                "Show leveledex { ItemLevel >= 81; BaseType \"Exalted Orb\"; };",
                "Show T1 { BaseType<>; };",
                "Show T2 { BaseType<>; };",
                "Show T3 { BaseType<>; };",
                "Show T4 { BaseType \"Exalted Orb\"; };",
                "# Show error;",
                "}",
                "Show T1 { BaseType \"Scroll of Wisdom\"; };"
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
                "Show leveledex { BaseType \"Obscure Orb\"; };",
                "}",
                "}",
            };

            var res = this.StringToExoFilter(input);

            Assert.IsNotNull(res);

            var commands0 = res.RootEntry.Scopes[0].Scopes[0].Scopes[0].ResolveAndSerialize(new ExoStyleDictionary()).ToList();

            Assert.AreEqual("SetTextColor 200 0 0 255", string.Join(" ", commands0[0]));
            Assert.AreEqual("SetBackgroundColor 255 255 255 255", string.Join(" ", commands0[1]));
            Assert.AreEqual("SetTextColor 255 0 0 255", string.Join(" ", commands0[2]));
            Assert.AreEqual("BaseType \"Obscure Orb\"", string.Join(" ", commands0[3]));
        }

        [Test]
        public void ExoProcessor_GlobalFunctionsSupport()
        {
            var input = new List<string>()
            {
                "AddTimeComment();"
            };

            var res = this.StringToFilterEntries(input);
            Assert.IsNotNull(res);
            Assert.AreEqual(1, res.Count);

            // worst test ever
            Assert.IsTrue(res[0].SerializeMergedString.Contains(DateTime.Now.Year.ToString()));
        }

        [Test]
        public void ExoProcessor_BasicMutatorTest()
        {
            var input = new List<string>()
            {
                "Func CurrencyBase(){ SetTextColor 200 0 0 255; BG 255 255 255 255; }",
                "Func IncubatorBase(){ SetTextColor 255 0 0 255; %D4; }",
                "Section Incubators : CurrencyBase, IncubatorBase, %HS5",
                "{",
                "Show leveledex { ItemLevel >= 81; BaseType \"Exalted Orb\"; };",
                "Show T4 { BaseType \"Exalted Orb\"; };",
                "# Show error;",
                "}",
                "Show T1 { BaseType \"Scroll of Wisdom\"; };"
            };

            var res = this.StringToExoFilter(input);

            Assert.IsNotNull(res);
            Assert.AreEqual(2, res.RootEntry.Scopes.Count);
            Assert.AreEqual(3, res.RootEntry.Scopes[0].Mutators.Count);
            Assert.AreEqual(3, res.RootEntry.Scopes[0].Scopes.Count);

            var commands0 = res.RootEntry.Scopes[0].Scopes[0].ResolveAndSerialize(new ExoStyleDictionary()).ToList();

            Assert.AreEqual("SetTextColor 200 0 0 255", string.Join(" ", commands0[0]));
            Assert.AreEqual("SetBackgroundColor 255 255 255 255", string.Join(" ", commands0[1]));
            Assert.AreEqual("SetTextColor 255 0 0 255", string.Join(" ", commands0[2]));

            // TODO: FIX COMMAND STACKING
            Assert.AreEqual("ItemLevel >= 81", string.Join(" ", commands0[5]));

            Assert.AreEqual("BaseType \"Scroll of Wisdom\"", res.RootEntry.Scopes[1].Commands[0].SerializeDebug());

            var resFilter = this.StringToFilterEntries(input);
            Assert.IsFalse(resFilter[2].Serialize().Contains("%HS5"));
            Assert.IsTrue(resFilter[0].Serialize()[0].Contains("%HS5"));
            Assert.IsTrue(resFilter[0].Serialize()[0].Contains("%D4"));
            Assert.IsTrue(resFilter[1].Serialize()[0].Contains("%HS5"));

            // TODO
            // Assert.IsTrue(resFilter[1].Serialize()[0].Contains("%D4"));
        }

        [Test]
        public void ExoProcessor_BasicFunctions()
        {
            var input = new List<string>()
            {
                "Func test (a,b) { SetBorderColor a 0 0 b; SetTextColor a 0 0 b; };",
                "Section Incubators",
                "{",
                "Show leveledex { ItemLevel >= 81; BaseType \"Exalted Orb\"; test(100,200); };",
                "Show T1 { test(100,200); };",
                "# Show error;",
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
                "Show leveledex { ItemLevel >= 81; BaseType \"Exalted Orb\"; test(3); };",
                "Show T1 { test(3); };",
                "# Show error;",
                "}"
            };

            var entries = this.StringToFilterEntries(input);
            var res = entries.Select(x => x.Serialize()).ToList();

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
                "Show T1 { BaseType ( \"Mirror\" + \"Wisdom\" ); %HS5; };"
            };

            var resFilter = this.StringToFilterEntries(input);

            Assert.AreEqual(1, resFilter[0].Header.GenerationTags.Count);
            Assert.AreEqual("HS", resFilter[0].Header.GenerationTags[0].Value);
            Assert.AreEqual("\tBaseType \"Mirror\" \"Wisdom\"", resFilter[0].Serialize()[1]);
        }

        [Test]
        public void ExoProcessor_ShowHideContinue()
        {
            var input = new List<string>()
            {
                "Show T1 { BaseType ( \"Mirror\" + \"Wisdom\" ); };",
                "Hide T2 { BaseType ( \"Mirror\" + \"Wisdom\" ); };",
                "Cont T3 { BaseType ( \"Mirror\" + \"Wisdom\" ); };"
            };

            var res = this.StringToFilterEntries(input);

            Assert.AreEqual(res[0].Serialize(), new List<string>() {"Show", "\tBaseType \"Mirror\" \"Wisdom\""});
            Assert.AreEqual(res[1].Serialize(), new List<string>() {"Hide", "\tBaseType \"Mirror\" \"Wisdom\""});
            Assert.AreEqual(res[2].Serialize(),
                new List<string>() {"Show", "\tBaseType \"Mirror\" \"Wisdom\"", "\tContinue"});
            Assert.AreEqual(res[0].Header.HeaderValue, "Show");
            Assert.AreEqual(res[1].Header.HeaderValue, "Hide");
            Assert.AreEqual(res[2].Header.HeaderValue, "Show");
        }

        [Test]
        public void ExoProcessor_VariableExpressionMerging()
        {
            var input = @"var a = ""wisdom"" ""fishing rod"" ""portal"";
                var b = ""mirror"" ""ex"" - ""ex"";
                var c = ""zero"" ""sword"";
                var d = ""wisdom"";
                Show T1 { BaseType ( a + b + c - d ); };";

            var res = this.StringToExoFilter(input.Split(System.Environment.NewLine).ToList());

            Assert.AreEqual(@"BaseType ""fishing rod"" ""mirror"" ""portal"" ""sword"" ""zero""",
                res.RootEntry.Scopes[0].Commands[0].SerializeDebug());
        }

        [Test]
        public void ExoProcessor_VariableArrayAccessing()
        {
            var input = @"var a = ""alpha"" ""beta"" ""gamma"";
                var b = ""x"" ""y"" ""z"" - ""y"";
                var c = ""wisdom"";
                Show T1 { BaseType ( a[2] + b + c[1] ); };";

            var res = this.StringToExoFilter(input.Split(System.Environment.NewLine).ToList());

            Assert.AreEqual(@"BaseType ""alpha"" ""beta"" ""wisdom"" ""x"" ""z""",
                res.RootEntry.Scopes[0].Commands[0].SerializeDebug());
        }

        [Test]
        public void ExoProcessor_SingleShowSerialization()
        {
            var input = new List<string>()
            {
                "Show T1 { BaseType<>; };"
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

                Show ONE { BaseType t1inc; };

                Section Incubators : IncubatorBase
                {
	                var t1inc = ""GAMMA"";
	                Show TWO { BaseType t1inc; };

	                Show THREE { ItemLevel >= 81; BaseType t1inc; };
	                # Show error;

	                Show FOUR { BaseType t1inc; };
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
                Assert.AreEqual("\"GAMMA\"",
                    res.RootEntry.Scopes[2].Variables["t1inc"].Serialize(res.RootEntry.Scopes[1]));

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

        [Test]
        public void LargeCommentTest()
        {
            var testString =
                @"#===============================================================================================================
# NeverSink's Indepth Loot Filter - for Path of Exile
#===============================================================================================================
# VERSION:  7.10.5
# TYPE:     SEED
# STYLE:    NORMAL
# AUTHOR:   NeverSink
# BUILDNOTES: Filter generated with NeverSink's FilterpolishZ
#
#------------------------------------
# LINKS TO LATEST VERSION AND FILTER EDITOR
#------------------------------------
#
# EDIT/CUSTOMIZE FILTER ON: 	https://www.FilterBlade.xyz
# GET THE LATEST VERSION ON: 	https://www.FilterBlade.xyz or https://github.com/NeverSinkDev/NeverSink-Filter
# POE FORUM THREAD: 			https://goo.gl/oQn4EN
#
#------------------------------------
# SUPPORT THE DEVELOPMENT:
#------------------------------------
#
# SUPPORT ME ON PATREON: 		https://www.patreon.com/Neversink
# SUPPORT THE FILTERBLADE TEAM: https://www.filterblade.xyz/About
#
#------------------------------------
# INSTALLATION / UPDATE :
#------------------------------------
#
# 0) It's recommended to check for updates once a month or at least before new leagues, to receive economy finetuning and new features!
# 1) Paste this file into the following folder: %userprofile%/Documents/My Games/Path of Exile
# 2) INGAME: Escape -> Options -> UI -> Scroll down -> Select the filter from the Dropdown box
#
#------------------------------------
# CONTACT - if you want to get notifications about updates or just get in touch:
#------------------------------------
# PLEASE READ THE FAQ ON https://goo.gl/oQn4EN BEFORE ASKING QUESTIONS
#
# TWITTER: @NeverSinkGaming
# REDDIT:  NeverSinkDev
# GITHUB:  NeverSinkDev
# EMAIL :  NeverSinkGaming-at-gmail.com

#------------------------------------
# [0702] Layer - T2 - ECONOMY""
#------------------------------------

Func IncubatorBase() { HasInfluence ""Shaper"" ""Elder""; };
var t1inc = ""Diviner's Incubator"" ""Enchanted Incubator"";
# var t1inc = ""Geomancer's Incubator"" ""Thaumaturge's Incubator"" ""Time-Lost Incubator"" 

Hide ONE { BaseType t1inc; };

Section Incubators : IncubatorBase
{
	var t1inc = ""Foreboding Incubator"";
	Show TWO { BaseType t1inc; };
	Show THREE { ItemLevel >= 81; BaseType t1inc; };
	Show FOUR { BaseType t1inc; };
	# Show error;
}";
                var res = this.StringToFilterEntries(testString.Split(System.Environment.NewLine).ToList());
                Assert.AreEqual(6, res.Count);
        }
    }
}
