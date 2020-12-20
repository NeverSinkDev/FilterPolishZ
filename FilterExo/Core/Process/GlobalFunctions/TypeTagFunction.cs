using System;
using System.Collections.Generic;
using System.Text;
using FilterExo.Core.PreProcess.Commands;
using FilterExo.Model;

namespace FilterExo.Core.Process.GlobalFunctions
{
    public class TypeTagFunction : IExoGlobalFunction
    {
        public string Name => "Type";

        public List<ExoAtom> Execute(ExoBlock content, ExoExpressionCommand caller)
        {
            //caller.Parent.MetaTags.Add("%HS5");
            return new List<ExoAtom>(){ new ExoAtom("%HS5") };
        }
    }
}
