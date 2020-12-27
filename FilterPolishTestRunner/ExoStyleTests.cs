using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FilterCore;
using FilterCore.Entry;
using FilterExo;
using FilterExo.Core.Parsing;
using FilterExo.Core.Structure;

namespace FilterPolishTestRunner
{
    [TestFixture]
    public class ExoStyleTests
    {
        private ExoBundle Bundle;

        private List<string> BasicStyleFile = new List<string>();

        [SetUp]
        public void SetUp()
        {
            BasicStyleFile = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "\\TestFiles\\basicstyle.filter").Split(System.Environment.NewLine).ToList();

            // AutoTier
            Bundle = new ExoBundle();
            var metaFilter = new List<string>()
            {
                "#------------------------------------",
                "#   [4913] Incubator (filter code)",
                "#------------------------------------",
                "",
                "Func IncubatorBase(){ Class Incubator; Tierlist(\"incubator\"); AutoTier(); }",
                "Section Incubator : IncubatorBase",
                "{",
                "\tvar IncuHiLevel = 81;",
                "\tvar HiLevelIncus = \"Celestial Armoursmith's Incubator\" \"Celestial Blacksmith's Incubator\" \"Celestial Jeweller's Incubator\" \"Enchanted Incubator\" \"Fragmented Incubator\" \"Otherworldly Incubator\";",
                "",
                "\tShow leveledex { ItemLevel >= IncuHiLevel; BaseType HiLevelIncus; %HS5;  };",
                "\tShow t1 { BaseType auto; };",
                "\tShow t2 { BaseType auto; };",
                "\tShow t3 { BaseType auto; };",
                "\tShow t4 { BaseType auto; };",
                "\tShow restex { Empty(); };",
                "}"
            };

            var input = metaFilter.Select(x => x).ToList();

            Bundle.SetInput(input)
                .Tokenize()
                .Structurize()
                .PreProcess();
        }

        [Test]
        public void MetaFilterIntegrityCheck()
        {
            var output = Bundle.Process();

            Assert.AreEqual(7, output.Count);
            var content = output.Where(x => x.Header.Type == FilterGenerationConfig.FilterEntryType.Content).ToList();
            
            Assert.IsTrue(content.Count == 6);
            Assert.IsTrue(content.All(x => x.Header.IsActive));
            Assert.IsTrue(content.All(x => !x.Header.IsFrozen));
            Assert.IsTrue(content.All(x => x.Header.HeaderValue == "Show"));
            Assert.IsTrue(content.All(x => x.Content.Content.ContainsKey("Class")));
            Assert.IsTrue(content.All(x => x.SerializeMergedString.Contains("$type->incubator")));

            var serOutput = output.SelectMany(x => x.Serialize()).ToList();

            Assert.NotNull(serOutput);
            Assert.AreEqual(21, serOutput.Count);
        }

        [Test]
        public void FileIntegrityTest()
        {
            Assert.IsNotEmpty(this.BasicStyleFile);
        }

        [Test]
        public void ParseBasicStyle()
        {
            var styleBundle = new ExoBundle()
                .SetInput(this.BasicStyleFile)
                .Tokenize()
                .Structurize()
                .PreProcess();

            Assert.IsNotNull(styleBundle);
        }

    }
}
