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
                var expr = new ExoExpressionCommand(item);
                expr.Parent = block;
                expr.Source = FilterExoConfig.ExoExpressionCommandSource.mutator;
                block.Mutators.Add(expr);
            }
        }
    }
}
