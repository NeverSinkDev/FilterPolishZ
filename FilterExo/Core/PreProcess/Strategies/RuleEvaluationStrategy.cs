using FilterExo.Core.PreProcess.Commands;
using FilterExo.Core.Structure;
using FilterExo.Model;
using FilterPolishUtil.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilterExo.Core.PreProcess.Strategies
{
    public class RuleEvaluationStrategy : IExpressionEvaluationStrategy
    {
        public bool Match(ExpressionBuilder builder)
        {
            var descriptor = builder.Owner.ReadCursor.GetFirstPropertyDescriptor();
            var validDescriptors = new HashSet<string>(){ "Show", "Hide", "Cont", "ConH" };

            return validDescriptors.Contains(descriptor);
        }

        public void Execute(ExpressionBuilder builder)
        {
            var result = new List<ExoExpressionCommand>();
            var descriptor = builder.Owner.ReadCursor.GetFirstPropertyDescriptor();

            // treat a whole entry
            for (int j = 0; j < builder.expressions.Count; j++)
            {
                TreatSinglePage(builder.expressions[j]);
            }

            // treat a single line
            void TreatSinglePage(List<StructureExpr> page)
            {
                List<string> rawtokens = new List<string>();

                for (int i = 0; i < page.Count; i++)
                {
                    var current = page[i];

                    if (current.Mode == FilterExoConfig.StructurizerMode.comm)
                    {
                        continue;
                    }

                    if (i == 0)
                    {
                        if (current?.Value == null)
                        {
                            LoggingFacade.LogDebug("No VALUE on: " + current.ToString());
                            continue;
                        }
                    }

                    rawtokens.Add(current.Value);
                }

                //var command = rawtokens.First();
                //rawtokens.RemoveAt(0);

                var filterCommand = new ExoExpressionCommand(rawtokens);
                result.Add(filterCommand);
            }

            // Resolve rule into an ExoBlock
            var block = new ExoBlock {Parent = builder.Owner.WriteCursor};

            builder.Owner.WriteCursor.Scopes.Add(block);

            block.Name = builder.Owner.ReadCursor.PropertyExpression[1].Value.ToLower();
            block.DescriptorCommand = descriptor;
            foreach (var item in result)
            {
                block.AddCommand(item);
            }
        }
    }
}
