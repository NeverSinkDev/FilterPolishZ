using FilterCore;
using FilterCore.Line;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilterPolishTestRunner.Util
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
