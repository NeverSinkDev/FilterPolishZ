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
        private ExoBundle FilterBundle = new ExoBundle();
        private ExoBundle StyleBundle = new ExoBundle();

        private List<string> IncubatorStyle = new List<string>();
        private List<string> IncubatorMeta = new List<string>();

        [SetUp]
        public void SetUp()
        {
            IncubatorMeta = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "\\TestFiles\\incubatorfilter01.filter").Split(System.Environment.NewLine).ToList();
            IncubatorStyle = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "\\TestFiles\\incubatorstyle01.filter").Split(System.Environment.NewLine).ToList();

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
        public void MetaFilterIntegrityCheck()
        {
            var output = FilterBundle.Process();

            Assert.AreEqual(14, output.Count);
            var content = output.Where(x => x.Header.Type == FilterGenerationConfig.FilterEntryType.Content).ToList();
            
            Assert.IsTrue(content.Count == 6);
            Assert.IsTrue(content.All(x => x.Header.IsActive));
            Assert.IsTrue(content.All(x => !x.Header.IsFrozen));
            Assert.IsTrue(content.All(x => x.Header.HeaderValue == "Show"));
            Assert.IsTrue(content.All(x => x.Content.Content.ContainsKey("Class")));
            Assert.IsTrue(content.All(x => x.SerializeMergedString.Contains("$type->incubator")));

            var serOutput = output.SelectMany(x => x.Serialize()).ToList();

            Assert.NotNull(serOutput);
            Assert.AreEqual(28, serOutput.Count);
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

            Assert.AreEqual("OK", results.FindByTag("t1").IsContaining("BaseType auto", "SetFontSize 45", "SetTextColor 255 0 0 255"));
            Assert.AreEqual("OK", results.FindByTag("t2").IsContaining("BaseType auto", "SetFontSize 45", "SetBackgroundColor 240 90 35 255", "MinimapIcon 1 Red Triangle"));
        }

    }
}
