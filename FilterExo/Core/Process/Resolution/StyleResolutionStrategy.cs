using System;
using System.Collections.Generic;
using System.Text;
using FilterExo.Model;

namespace FilterExo.Core.Process.StyleResoluton
{
    public static class StyleResolutionStrategy
    {
        public static List<List<string>> Execute(ExoBlock currentRule, ExoStyleDictionary style)
        {
            var result = new List<List<string>>();

            var currentRuleName = currentRule.Name;
            var parentRuleName = currentRule.Parent.Name;

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
