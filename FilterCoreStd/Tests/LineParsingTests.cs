using FilterCore.Line;
using FilterDomain.LineStrategy;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterCore.Tests
{
    [TestFixture]
    public class LineParsingTests
    {

        [Test]
        public void ParseEnumLine()
        {
            var line = TestUtils.ParseFilterLine("BaseType \"Opal Ring\" \"Steel Ring\" \"Crystal Belt\"");

            Assert.That(line, Is.Not.Null);
            Assert.That(line.Ident, Is.EqualTo("BaseType"));
            Assert.That((line.Value as EnumValueContainer).Value.Select(x => x.value), Is.SubsetOf(new List<string>() { "Opal Ring", "Steel Ring", "Crystal Belt" }));
            Assert.That((line.Value as EnumValueContainer).Value.Select(x => x.isQuoted), Is.All.EqualTo(true));
            Assert.That(line.Comment, Is.Empty);
        }

        [Test]
        public void ParseBoolLine()
        {
            var line = TestUtils.ParseFilterLine("ShapedMap True");

            Assert.That(line, Is.Not.Null);
            Assert.That(line.Ident, Is.EqualTo("ShapedMap"));
            Assert.That((line.Value as BoolValueContainer).Value, Is.True);
            Assert.That(line.Comment, Is.Empty);
        }

        [Test]
        public void ParseColorLineNoOpacity()
        {
            var line = TestUtils.ParseFilterLine("    SetBackgroundColor 255 0 0 #BACKGROUND: TEST");
            var value = line.Value as ColorValueContainer;

            Assert.That(line, Is.Not.Null);
            Assert.That(line.Ident, Is.EqualTo("SetBackgroundColor"));
            Assert.That((value.R), Is.EqualTo(255));
            Assert.That((value.G), Is.EqualTo(0));
            Assert.That((value.B), Is.EqualTo(0));
            Assert.That((value.O), Is.EqualTo(-1));
            Assert.That(line.Comment, Is.EqualTo("BACKGROUND: TEST"));
        }

        [Test]
        public void ParseColorLineWithOpacity()
        {
            var line = TestUtils.ParseFilterLine("    SetTextColor 251 252 253 254 #BACKGROUND: TEST");
            var value = line.Value as ColorValueContainer;

            Assert.That(line, Is.Not.Null);
            Assert.That(line.Ident, Is.EqualTo("SetTextColor"));
            Assert.That((value.R), Is.EqualTo(251));
            Assert.That((value.G), Is.EqualTo(252));
            Assert.That((value.B), Is.EqualTo(253));
            Assert.That((value.O), Is.EqualTo(254));
            Assert.That(line.Comment, Is.EqualTo("BACKGROUND: TEST"));
        }

        [Test]
        public void ParseVariableLine()
        {
            var line = TestUtils.ParseFilterLine("	MinimapIcon 0 Blue Diamond");
            var value = line.Value as VariableValueContainer;

            Assert.That(line, Is.Not.Null);
            Assert.That(line.Ident, Is.EqualTo("MinimapIcon"));
            Assert.That((line.Value as VariableValueContainer).Value.Select(x => x.value), Is.EqualTo(new List<string>() { "0", "Blue", "Diamond" }));
        }

        [Test]
        public void ParseNumericLine()
        {
            var line = TestUtils.ParseFilterLine("	Rarity >= Rare");
            var value = line.Value as NumericValueContainer;

            Assert.That(line, Is.Not.Null);
            Assert.That(line.Ident, Is.EqualTo("Rarity"));
            Assert.That(value.Operator, Is.EqualTo(">="));
            Assert.That(value.Value, Is.EqualTo("Rare"));
        }

        [Test]
        public void ParseCommentWithTabs()
        {
            const string originalLine = "# \tKey:\t\tval";
            var line = TestUtils.ParseFilterLine(originalLine);
            var result = line.Serialize();
            
            Assert.That("#" + result, Is.EqualTo(originalLine));
            Assert.That(result.Contains(originalLine), Is.True);
        }
    }
}
