using FilterExo.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilterExo.Core.PreProcess.Strategies
{
    public interface IExpressionEvaluationStrategy
    {
        bool Match(ExpressionBuilder builder);
        void Execute(ExpressionBuilder builder);
    }
}
