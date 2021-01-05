using FilterExo.Core.PreProcess.Commands;
using FilterExo.Core.Structure;
using FilterExo.Model;
using FilterPolishUtil.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilterExo.Core.PreProcess.Strategies
{
    public static class ExpressionMutatorUtil
    {
        public static void ExpandBlockWithMutators(ExoBlock block, List<StructureExpr> expressions, string context)
        {
            // we only evaluate mutators that start after a ":"
            var rawmutators = expressions.SkipWhile(x => x.PrimitiveValue.value != ":").Skip(1).ToList();
            
            var splitmutators = rawmutators.SplitDivide(x => x.PrimitiveValue.value == ",");

            foreach (var item in splitmutators)
            {
                if (item.Count == 0)
                {
                    continue;
                }

                var potentialSection = block.FindChildSectionFromRoot(item[0].Value).ToList();

                ExoExpressionCommand expr;
                if (potentialSection.Count == 1)
                {
                    block.LinkedBlocks.Add(potentialSection[0]);
                    return;
                }
                else if (item.Count == 1 && block.IsFunction(item[0].Value))
                {
                    expr = new ExoExpressionCommand(new List<string> { item[0].Value, "(", ")" });
                }
                else
                {
                    expr = new ExoExpressionCommand(item);
                }
                if (expr.Values.Count > 0)
                {
                    expr.Parent = block;
                    expr.Source = FilterExoConfig.ExoExpressionCommandSource.mutator;
                    block.Mutators.Add(expr);
                }
            }
        }
    }
}
