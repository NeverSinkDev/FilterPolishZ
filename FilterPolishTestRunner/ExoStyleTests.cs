using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
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
            this.Bundle = new ExoBundle();
        }

        public List<FilterEntry> Process(List<string> data)
        {
            var result = Bundle.SetInput(data)
                .Tokenize()
                .Structurize()
                .PreProcess()
                .Process();

            return result;
        }

        [Test]
        public void SimpleStyleCheck()
        {
            List<string> input = new List<string>();
            var entries = Process(input);

            Assert.NotNull(Bundle);
        }

    }
}
