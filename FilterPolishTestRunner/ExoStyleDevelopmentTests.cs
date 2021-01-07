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
    public class ExoStyleDevelopmentTests
    {
        private ExoBundle FilterBundle = new ExoBundle();
        private ExoBundle StyleBundle = new ExoBundle();

        private List<string> IncubatorStyle = new List<string>();
        private List<string> IncubatorMeta = new List<string>();

        [SetUp]
        public void SetUp()
        {
            IncubatorMeta = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "\\TestFiles\\alphafilter.filter").Split(System.Environment.NewLine).ToList();
            IncubatorStyle = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "\\TestFiles\\alphastyle.filter").Split(System.Environment.NewLine).ToList();

            var style = IncubatorStyle.Select(x => x).ToList();
            var meta = IncubatorMeta.Select(x => x).ToList();

            StyleBundle = new ExoBundle();
            FilterBundle = new ExoBundle();

            StyleBundle.SetInput(style)
                .Tokenize()
                .Structurize()
                .PreProcess();

            FilterBundle.SetInput(meta)
                .Tokenize()
                .Structurize()
                .PreProcess();
        }

        [Test]
        public void FileIntegrityTest()
        {
            Assert.IsNotEmpty(this.IncubatorStyle);
        }

        [Test]
        public void ParseBasicStyle()
        {
            var dict = StyleBundle.StyleProcess();

            Assert.IsNotNull(dict);

            FilterBundle.DefineStyleDictionary(dict);
            var results = FilterBundle.Process();

            var serializedResults = results.SelectMany(x => x.Serialize()).ToList();
            Filter tempFilter = new Filter(serializedResults);
            Assert.IsNotNull(results);

            var filterText = tempFilter.Serialize();
            Assert.IsNotNull(filterText);
        }

    }
}
