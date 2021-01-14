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


            var metaFilter = FileWork.ReadLinesFromFile("C:\\FilterOutput\\RESULTS\\ADDITIONAL-FILES\\SeedFilter\\MetaPrototype.filter");
            var metaStyle = FileWork.ReadLinesFromFile("C:\\FilterOutput\\RESULTS\\ADDITIONAL-FILES\\SeedFilter\\StylePrototype.filter");


            var style = metaStyle.Select(x => x).ToList();
            var meta = metaFilter.Select(x => x).ToList();

            var styleBundle = new ExoBundle();
            var filterBundle = new ExoBundle();

            //styleBundle.SetInput(style)
            //    .Tokenize()
            //    .Structurize()
            //    .PreProcess();

            filterBundle.SetInput(meta)
                .Tokenize()
                .Structurize()
                .PreProcess();

            //var dict = styleBundle.StyleProcess();
            //filterBundle.DefineStyleDictionary(dict);

            var results = filterBundle.Process();

            var serializedResults = results.SelectMany(x => x.Serialize()).ToList();
            Filter tempFilter = new Filter(serializedResults);
            Assert.IsNotNull(results);

            var filterText = tempFilter.Serialize();
            var filterJoinedString = string.Join(System.Environment.NewLine, filterText);

            await FileWork.WriteTextAsync("C:\\FilterOutput\\RESULTS\\ADDITIONAL-FILES\\SeedFilter\\ExoOutput.filter", filterJoinedString);
        }
    }
}
