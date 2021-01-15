using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FilterExo.Core.PreProcess.Commands;
using FilterExo.Core.Process.StyleResoluton;
using FilterExo.Model;
using FilterPolishUtil;

namespace FilterExo.Core.Process.GlobalFunctions.StyleFunctions
{
    public class ApplyStyleFunction : IExoGlobalFunction
    {
        public string Name { get; } = "apply";
        public List<ExoAtom> Execute(List<ExoAtom> variables, ExoExpressionCommand caller)
        {
            var dict = ExoStyleProcessor.ConstructedDictionary;
            var style = ExoStyleProcessor.WorkedStyleFile;
            var vars = variables.Select(x => x.Serialize(caller.Parent)).ToList();

            string[] separators = { "=>" };
            var mergedVars = string.Join("", vars);
            var results = mergedVars.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList();
            var styleName = results[0].Trim();
            var sectionNames = results[1].Trim();

            var rules = ExoStylePiece.FromExpression(styleName, sectionNames, this.Name, caller);

            dict.Rules.AddRange(rules);

            return new List<ExoAtom>();
        }
    }
}
