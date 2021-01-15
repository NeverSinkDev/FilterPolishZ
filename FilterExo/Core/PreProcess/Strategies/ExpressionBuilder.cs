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
using FilterPolishUtil;

namespace FilterExo.Core.PreProcess.Strategies
{
    public class ExpressionBuilder
    {
        public ExpressionBuilder(ExoPreProcessor owner)
        {
            this.Owner = owner;
        }

        public ExoPreProcessor Owner;

        public static List<IExpressionEvaluationStrategy> ExplicitStrategies = new List<IExpressionEvaluationStrategy>()
        {
            new RuleEvaluationStrategy(),
            new FuncEvaluationStrategy(),
            new UnknownStepEvaluationStrategy()
        };

        public static List<IExpressionEvaluationStrategy> ImplicitStrategies = new List<IExpressionEvaluationStrategy>()
        {
            new VarEvaluationStrategy(),
            new CommentEvaluationStrategy(),
            new SnippetExecutionStrategy()
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

        public bool ExecuteExplicit()
        {
            foreach (var item in ExplicitStrategies)
            {
                if (item.Match(this))
                {
                    item.Execute(this);
                    return true;
                }
            }

            return false;
        }

        public bool ExecuteImplicit()
        {
            foreach (var item in ImplicitStrategies)
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

    public class UnknownStepEvaluationStrategy : IExpressionEvaluationStrategy
    {
        public bool Match(ExpressionBuilder builder)
        {
            var descriptor = builder.Owner.ReadCursor.GetFirstPropertyDescriptor();
            if (descriptor == string.Empty || descriptor == "Section" || descriptor == "Style")
            {
                return false;
            }

            return true;
        }

        public void Execute(ExpressionBuilder builder)
        {
            var descriptor = builder.Owner.ReadCursor.GetFirstPropertyDescriptor();
            TraceUtility.Throw($"Unknown descriptor: {descriptor}");
        }
    }
}
