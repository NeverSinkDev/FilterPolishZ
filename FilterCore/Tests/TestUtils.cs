using FilterCore.Line;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterCore.Tests
{
    public static class TestUtils
    {
        public static IFilterLine ParseFilterLine(string input)
        {
            var filter = new Filter(new List<string>() { "Show", input });
            return filter.FilterLines[1];
        }
    }
}
