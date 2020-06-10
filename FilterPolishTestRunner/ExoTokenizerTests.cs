using FilterExo.Core.Parsing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilterPolishTestRunner
{
    [TestFixture]
    public class ExoTokenizerTests
    {
        public ExoTokenizer Tokenizer;

        [SetUp]
        public void SetUp()
        {
            Tokenizer = new ExoTokenizer();
        }

        [Test]
        public void Tokenizer_TestResultCount()
        {
            var input = new List<string>()
            {
                "# [4112] Incubator",
                "",
                "{ => - < = ",
                "Rule leveledex { ItemLevel >= 81; BaseType <>; };",
                "}"
            };

            Tokenizer.Execute(input);
            Assert.AreEqual(5, Tokenizer.Results.Count);
        }

        [Test]
        public void Tokenizer_TestComment()
        {
            var input = new List<string>()
            {
                "# [4112] < \" ' > \t >= = ??? Incubator"
            };

            Tokenizer.Execute(input);
            Assert.AreEqual(1, Tokenizer.Results[0].Count);
            Assert.AreEqual(FilterExo.FilterExoConfig.TokenizerMode.comment, Tokenizer.Results[0][0].type);
        }

        [Test]
        public void Tokenizer_TestOperatorTreatment()
        {
            var input = new List<string>()
            {
                "><= =>STRING;WORD ;;==;"
            };

            Tokenizer.Execute(input);
            Assert.AreEqual(10, Tokenizer.Results[0].Count);
        }

        [Test]
        public void Tokenizer_TestQuotes()
        {
            var input = new List<string>()
            {
                "\"#{#}#\" WORD W\"START\"};"
            };

            Tokenizer.Execute(input);
            Assert.AreEqual(6, Tokenizer.Results[0].Count);
            Assert.AreEqual(FilterExo.FilterExoConfig.TokenizerMode.quoted, Tokenizer.Results[0][0].type);
        }
    }
}
