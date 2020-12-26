using System;
using System.Collections.Generic;
using System.Text;
using FilterExo.Core.PreProcess.Commands;
using FilterExo.Model;

namespace FilterExo.Core.Process.GlobalFunctions
{
    public class EmptyFunction : IExoGlobalFunction
    {
        public string Name => "Empty";

        public List<ExoAtom> Execute(List<ExoAtom> content, ExoExpressionCommand caller)
        {
            return new List<ExoAtom>();
        }
    }
}
