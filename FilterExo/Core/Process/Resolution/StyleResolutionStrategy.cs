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

            var currentRuleName = currentRule.Name;
            var parentSectionName = currentRule.Parent.Name;

            var relevantRules = style.Rules.Where(x => string.Equals(x.attachmentRule, parentSectionName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (relevantRules.Count > 0)
            {
                if (!relevantRules[0].Block.Functions.ContainsKey(currentRuleName))
                {
                    return result;
                }

                var parent = relevantRules[0].Block;
                var functionName = parent.Functions[currentRuleName];
                var parameters = new PreProcess.Commands.Branch<ExoAtom>();
                var caller = relevantRules[0].Caller;

                var funcResults= functionName.GetFunction(parent).Execute(parameters, caller).ToList();

                foreach (var item in funcResults)
                {
                    result.Add(new ExoExpressionCommand(item).Serialize());
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
