using System;
using System.Collections.Generic;
using System.Text;
using FilterCore.FilterComponents.Entry;
using NUnit.Framework;

namespace FilterPolishTestRunner
{
    [TestFixture]
    public class FilterEntryBuilderTests
    {
        public FilterEntryBuilder Builder;

        [SetUp]
        public void Setup()
        {
            Builder = new FilterEntryBuilder();
            Builder.Reset();
            Builder.RestoreInitialValues();
        }

        [Test]
        public void BasicEntryBuilderIntegrity()
        {
            Builder.AddCommand("Show");
            Builder.AddCommand("Rarity >= Rare");

            var entry = Builder.Execute();
            var serialized = entry.Serialize();

            Assert.AreEqual(2, serialized.Count);
        }
    }
}
