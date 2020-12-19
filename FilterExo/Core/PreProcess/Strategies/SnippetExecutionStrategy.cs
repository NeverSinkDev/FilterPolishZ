using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FilterExo.Core.PreProcess.Commands;
using FilterExo.Core.Structure;
using FilterExo.Model;
using FilterPolishUtil.Extensions;
using FilterPolishUtil.Model;

namespace FilterExo.Core.PreProcess.Strategies
{
    public class SnippetExecutionStrategy : IExpressionEvaluationStrategy
    {
        public bool Match(ExpressionBuilder builder)
        {
            // detect function snippets
            if (builder.expressions.Count != 1) { return false; }

            if (builder.expressions[0].Last().Value != ")") { return false; }

            var name = builder.expressions[0][0].Value;
            return ExoBlock.GlobalFunctions.ContainsKey(name);
        }

        public void Execute(ExpressionBuilder builder)
        {
            var result = new List<ExoExpressionCommand>();
            var descriptor = "snippet";

            // treat a whole entry
            for (int j = 0; j < builder.expressions.Count; j++) { TreatSinglePage(builder.expressions[j]); }

            // treat a single line
            void TreatSinglePage(List<StructureExpr> page)
            {
                List<string> rawtokens = new List<string>();

                for (int i = 0; i < page.Count; i++)
                {
                    var current = page[i];

                    if (current.Mode == FilterExoConfig.StructurizerMode.comm) { continue; }

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

            block.Name = "snippet";
            block.DescriptorCommand = descriptor;

            block.MetaTags = new List<ExoAtom>();
            foreach (var item in result) { block.AddCommand(item); }
        }
    }
}
