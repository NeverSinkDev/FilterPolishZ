using FilterCore;
using FilterCore.Entry;
using FilterCore.Line;
using FilterCore.Line.Parsing;
using FilterExo.Core.Structure;
using FilterExo.Model;
using FilterPolishUtil.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Transactions;

namespace FilterExo.Core.PreProcess.Strategies
{
    public class ExpressionBuilder
    {
        public ExpressionBuilder(ExoPreProcessor owner)
        {
            this.Owner = owner;
        }

        public ExoPreProcessor Owner;

        public static List<IExpressionEvaluationStrategy> Strategies = new List<IExpressionEvaluationStrategy>()
        {
            new RuleEvaluationStrategy(),
            new VarEvaluationStrategy(),
            new FuncEvaluationStrategy()
        };

        public string Mode;

        public List<List<StructureExpr>> expressions = new List<List<StructureExpr>>();

        public ExpressionBuilder AddKeyWord(StructureExpr expr)
        {
            expressions.Last().Add(expr);

            if (this.expressions.Count == 1 && expr.Value == "var")
            {
                Mode = "var";
            }

            return this;
        }

        public ExpressionBuilder AddNewPage()
        {
            if (expressions.Count == 0)
            {
                expressions.Add(new List<StructureExpr>());
                return this;
            }

            if (expressions.Last().Count == 0)
            {
                return this;
            }

            expressions.Add(new List<StructureExpr>());
            return this;
        }

        public bool Execute()
        {
            foreach (var item in Strategies)
            {
                if (item.Match(this))
                {
                    item.Execute(this);
                    return true;
                }
            }

            return false;
        }
    }
}
