using System;
using System.Collections.Generic;
using System.Text;
using FilterExo.Core.PreProcess.Commands;
using FilterExo.Model;

namespace FilterExo.Core.Process.GlobalFunctions
{
    public interface IExoGlobalFunction
    {
        string Name { get; }
        List<ExoAtom> Execute(List<ExoAtom> variables, ExoExpressionCommand caller);
    }

    public static class EExoGlobalFunction
    {
        public static void Integrate(this IExoGlobalFunction me)
        {
            var function = new ExoFunction()
            {
                // Content = new ExoBlock() { Name = me.Name, Type = FilterExoConfig.ExoFilterType.generic },
                Name = me.Name.ToLower(),
                Type = ExoFunctionType.global,
                GlobalFunctionLink = me
            };

            var atom = new ExoAtom(function);
            ExoBlock.GlobalFunctions.Add(me.Name.ToLower(), atom);
        }
    }
}
