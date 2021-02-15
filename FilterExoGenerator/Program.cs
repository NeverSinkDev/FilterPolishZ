using System;
using System.Linq;
using System.Threading.Tasks;
using FilterCore;
using FilterExo;
using FilterPolishUtil;
using NUnit.Framework;

namespace FilterExoGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Generating ExoFilters!");


            var metaFilter = FileWork.ReadLinesFromFile("C:\\PROJECTS\\Filter-Precursors\\MetaPrototype.filter");
            var metaStyle = FileWork.ReadLinesFromFile("C:\\PROJECTS\\Filter-Precursors\\StylePrototype.filter");

            var style = metaStyle.Select(x => x).ToList();
            var meta = metaFilter.Select(x => x).ToList();

            var styleBundle = new ExoBundle();
            var filterBundle = new ExoBundle();

            styleBundle.SetInput(style)
                .Tokenize()
                .Structurize()
                .PreProcess();

            filterBundle.SetInput(meta)
                .Tokenize()
                .Structurize()
                .PreProcess();

            var styleDict = styleBundle.StyleProcess();
            filterBundle.DefineStyleDictionary(styleDict);

            var results = filterBundle.Process();

            var serializedResults = results.SelectMany(x => x.Serialize()).ToList();
            Filter tempFilter = new Filter(serializedResults);
            Assert.IsNotNull(results);

            var filterText = tempFilter.Serialize();
            var filterJoinedString = string.Join(System.Environment.NewLine, filterText);

            await FileWork.WriteTextAsync("C:\\PROJECTS\\Filter-Precursors\\ExoOutput.filter", filterJoinedString);
        }
    }
}
