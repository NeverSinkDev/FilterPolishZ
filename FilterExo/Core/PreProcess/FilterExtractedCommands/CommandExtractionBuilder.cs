using FilterCore;
using FilterCore.Entry;
using FilterCore.Line;
using FilterCore.Line.Parsing;
using FilterExo.Core.Structure;
using FilterPolishUtil.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FilterExo.Core.PreProcess.FilterExtractedCommands
{
    public class CommandExtractionBuilder
    {
        public List<StructureExpr> expressions = new List<StructureExpr>();

        public CommandExtractionBuilder AddKeyWord(StructureExpr expr)
        {
            expressions.Add(expr);
            return this;
        }

        public FilterEntry Execute()
        {
            var result = FilterEntry.CreateDataEntry("Show");

            if (expressions.Count == 0)
            {
                return result;
            }

            bool commandFound = false;

            List<string> rawtokens = new List<string>();

            for (int i = 0; i < expressions.Count; i++)
            {
                var current = expressions[i];

                if (i == 0)
                {
                    if (current?.Value == null)
                    {
                        LoggingFacade.LogDebug("No VALUE on: " + current.ToString());
                        return result;
                    }

                    if (FilterGenerationConfig.LineTypes.ContainsKey(current.Value))
                    {
                        commandFound = true;
                    }
                }

                if (CanResolveAtomicExpr(current))
                {
                    rawtokens.Add(ResolveAtomicExpression(current));
                }
                else
                {
                    rawtokens.Add(current.Value);
                }
            }
            
            var tokens = LineParser.TokenizeFilterLineString(string.Join(" ",rawtokens));
            var line = LineParser.GenerateFilterLine(tokens);

            result.Content.Add(line);
            return result;
        }

        public static bool CanResolveAtomicExpr(StructureExpr expr)
        {
            if (expr.PrimitiveValue.IsOperator)
            {
                return true;
            }

            return false;
        }

        public static string ResolveAtomicExpression(StructureExpr expr)
        {
            return "RESOLVED";
        }
    }
}
