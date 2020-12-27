using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FilterExo.Core.PreProcess.Commands;
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

            var rule = new ExoStylePiece();

            rule.attachmentRule = sectionNames;

            var section = style.RootEntry.FindChildSection(styleName).ToList();
            if (section.Count == 0)
            {
                TraceUtility.Throw($"Style section not found! {mergedVars}");
            }

            rule.Block = section[0];
            rule.metaRule = this.Name;

            dict.Rules.Add(rule);

            return new List<ExoAtom>();
        }
    }
}
