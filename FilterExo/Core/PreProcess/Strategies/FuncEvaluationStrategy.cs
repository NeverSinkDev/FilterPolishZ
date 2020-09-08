using FilterExo.Core.PreProcess.Commands;
using FilterExo.Core.Structure;
using FilterExo.Model;
using FilterPolishUtil.Extensions;
using FilterPolishUtil.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilterExo.Core.PreProcess.Strategies
{
    public class FuncEvaluationStrategy : IExpressionEvaluationStrategy
    {
        public void Execute(ExpressionBuilder builder)
        {

            var properties = builder.Owner.ReadCursor.PropertyExpression;
            var functionName = properties[1];

            var split = properties.SelectInnerContents(
                x => x.Value == "(",
                x => x.Value == ")");

            List<string> varNames = new List<string>();
            if (split != null)
            {
                var vars = split.SplitDivide(x => x.Value == ",");

                if (vars != null && vars.Count >= 1)
                {
                    varNames = vars.Select(x => x.FirstOrDefault().Value).ToList();
                }
            }

            var result = new List<ExoExpressionCommand>();

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

                var filterCommand = new ExoExpressionCommand(rawtokens);
                result.Add(filterCommand);
            }

            var newEntry = new ExoBlock();
            // newEntry.Parent = builder.Owner.WriteCursor;
            //builder.Owner.WriteCursor.Scopes.Add(newEntry);

            foreach (var item in result)
            {
                newEntry.AddCommand(item);
            }



            // Resolve rule into a "Show" filter entry.
            var function = new ExoFunction
            {
                Name = functionName.Value,
                Content = newEntry,
                Variables = varNames
            };

            builder.Owner.WriteCursor.Functions.Add(functionName.Value, new ExoAtom(function));
        }

        public bool Match(ExpressionBuilder builder)
        {
            var descriptor = builder.Owner.ReadCursor.GetFirstPropertyDescriptor();
            return descriptor == "Func";
        }
    }
}
