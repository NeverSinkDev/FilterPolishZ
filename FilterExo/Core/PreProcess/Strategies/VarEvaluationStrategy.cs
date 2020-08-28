﻿using FilterExo.Core.Structure;
using FilterExo.Model;
using static FilterPolishUtil.TraceUtility;
using FilterPolishUtil.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FilterPolishUtil;

namespace FilterExo.Core.PreProcess.Strategies
{
    public class VarEvaluationStrategy : IExpressionEvaluationStrategy
    {
        public void Execute(ExpressionBuilder builder)
        {
            Check(builder.expressions.Count != 1, "Variable has more than 1 page");

            var exprRoot = builder.expressions[0];

            var split = exprRoot.SplitBy(x => x.Value == "=");
            var variableInfo = this.HandleBefore(split.before);
            var variableContent = this.HandleAfter(split.after);

            StoreVariable(builder, variableInfo, variableContent);
        }

        private static void StoreVariable(ExpressionBuilder builder, (string name, string type) variableInfo, List<string> variableContent)
        {
            Check(FilterCore.FilterGenerationConfig.TierTagSort.ContainsKey(variableInfo.name), "variable uses reserved name!");
            Check(FilterCore.FilterGenerationConfig.LineTypesSort.ContainsKey(variableInfo.name), "variable uses reserved name!");
            Check(FilterCore.FilterGenerationConfig.ValidRarities.Contains(variableInfo.name), "variable uses reserved name!");
            Check(variableInfo.name.ContainsSpecialCharacters(), "variable uses invalid characters!");

            builder.Owner.WriteCursor.Variables.Add(
                variableInfo.name, 
                new SimpleExoVariable(variableContent));
        }

        private List<string> HandleAfter(List<StructureExpr> after)
        {
            var results = new List<string>();
            foreach (var item in after)
            {
                Check(item.Mode != FilterExoConfig.StructurizerMode.atom, "unknown scope type during variable treatment!");
                Check(item.Value == string.Empty || item.Value == null, "valueless variable content!");

                results.Add(item.Value);
            }

            return results;
        }

        private (string name, string type) HandleBefore(List<StructureExpr> before)
        {
            Check(before.Count != 2, "not 2 params in befor-var treatment");
            
            return (before[1].Value, before[0].Value);
        }

        public bool Match(ExpressionBuilder builder)
        {
            if (builder.Mode == "var")
            {
                return true;
            }

            return false;
       }
    }
}
