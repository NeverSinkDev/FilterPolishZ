using FilterExo.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FilterExo.Core.PreProcess.Strategies
{
    public class CommentEvaluationStrategy : IExpressionEvaluationStrategy
    {
        public void Execute(ExpressionBuilder builder)
        {
            var cursor = builder.Owner.WriteCursor;

            ExoBlock block;
            if (cursor.Scopes.Count > 0 && cursor.Scopes.Last().Type == FilterExoConfig.ExoFilterType.comment)
            {
                block = cursor.Scopes.Last();
            }
            else
            {
                block = new ExoBlock();
                block.Type = FilterExoConfig.ExoFilterType.comment;
                cursor.Scopes.Add(block);
                block.Parent = cursor;
            }

            foreach (var itemset in builder.expressions)
            {
                foreach (var item in itemset)
                {
                    block.SimpleComments.Add(item.Value);
                }
            }

        }

        public bool Match(ExpressionBuilder builder)
        {
            if (builder.expressions.Count > 0 && builder.expressions.All(x => x.All(y => y.Mode == FilterExoConfig.StructurizerMode.comm)))
            {
                return true;
            }

            return false;
        }
    }
}
