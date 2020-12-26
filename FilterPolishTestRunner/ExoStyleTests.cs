using NUnit.Framework;
using System;
using System.Collections.Generic;
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

        [SetUp]
        public void SetUp()
        {
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
            Assert.IsTrue(output.Count(x => x.Header.Type == FilterGenerationConfig.FilterEntryType.Content) == 6);

            var serOutput = output.SelectMany(x => x.Serialize()).ToList();
            Assert.NotNull(serOutput);
        }

    }
}
