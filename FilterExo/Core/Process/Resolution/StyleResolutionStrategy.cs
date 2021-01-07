using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FilterExo.Core.PreProcess.Commands;
using FilterExo.Model;

namespace FilterExo.Core.Process.StyleResoluton
{
    public static class StyleResolutionStrategy
    {
        public static List<List<string>> Execute(ExoBlock currentRule, ExoStyleDictionary style)
        {
            var result = new List<List<string>>();

            var currentRuleName = currentRule.Name;          // rulename: "t1"
            var parentSectionName = currentRule.Parent.Name; // sectionName: "Incubators"

            // match the filter-rules (such as T1) to the style rules. Handle different application strategies here later
            var relevantRules = style.Rules.Where(x => string.Equals(x.attachmentRule, parentSectionName, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var styleRule in relevantRules)
            {
                var functions = styleRule.Block.YieldLinkedFunctions(currentRuleName).ToList();
                if (functions.Count == 0)
                {
                    continue;
                }

                var parent = styleRule.Block;
                var parameters = new PreProcess.Commands.Branch<ExoAtom>();
                var caller = styleRule.Caller;

                var funcResults = new List<List<ExoAtom>>();
                foreach (var function in functions)
                {
                    caller.Executor = parent;
                    caller.Parent = parent;
                    function.Content.Parent = parent;
                    funcResults.AddRange(function.Execute(parameters, caller).ToList());
                }

                foreach (var item in funcResults)
                {
                    result.Add(new ExoExpressionCommand(item) { Parent = parent, Executor = parent }.Serialize());
                }
            }

            // 1) iterate through the syle exoblock
            // 2) find rules that apply to our metaBlock
            // 3) get the corresponding sections for the applying rules
            // 4) return the results

            // BEFORE THAT
            // 1) iterate the style block
            // 2) create rules that tell, which metaBlock they apply to
            // 3) ...

            return result;
        }
    }
}
