using FilterExo.Core.Parsing;
using FilterExo.Core.Structure;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilterPolishTestRunner
{
    [TestFixture]
    public class ExoStructurizerTests
    {
        private Structurizer Structurizer;
        private ExoTokenizer Tokenizer;
        private StructurizerDebugger StructurizerDebugger;

        [SetUp]
        public void SetUp()
        {
            Tokenizer = new ExoTokenizer();
            Structurizer = new Structurizer();
            StructurizerDebugger = new StructurizerDebugger();
        }

        [Test]
        public void StructurizerResultSaneTest()
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
                "Rule error;",
                "}"
            };

            var result = Structurize(input);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Children.Count >= 1);
            Assert.IsTrue(result.Mode == FilterExo.FilterExoConfig.StructurizerMode.root);
        }

        [Test]
        public void StructurizerDictRootLevelTests()
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
                "Rule error;",
                "}"
            };

            var result = Structurize(input);
            var treeDict = StructurizerDebugger.SelectOnTree(result, x => x.Mode);

            Assert.IsNotNull(treeDict);
            Assert.IsTrue(treeDict["r"] == FilterExo.FilterExoConfig.StructurizerMode.root);
        }

        private StructureExpr Structurize(List<string> input)
        {
            Tokenizer.Execute(input);
            return Structurizer.Execute(Tokenizer.Results);
        }
    }
}
