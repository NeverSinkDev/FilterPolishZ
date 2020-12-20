using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FilterExo.Core.PreProcess.Commands;
using FilterExo.Model;
using FilterPolishUtil;

namespace FilterExo.Core.Process.GlobalFunctions
{
    public class TypeTagFunction : IExoGlobalFunction
    {
        public string Name => "Tierlist";

        public List<ExoAtom> Execute(List<ExoAtom> content, ExoExpressionCommand caller)
        {
            var variables = content.Select(x => x.Serialize(caller.Parent)).ToList();
            TraceUtility.Check(variables.Count != 1, "wrong variables count");
            
            return new List<ExoAtom>(){ new ExoAtom("$type->" + variables[0].DeQuote()) };
        }
    }

    public class TierTagFunction : IExoGlobalFunction
    {
        public string Name => "Tier";

        public List<ExoAtom> Execute(List<ExoAtom> content, ExoExpressionCommand caller)
        {
            var variables = content.Select(x => x.Serialize(caller.Parent)).ToList();
            TraceUtility.Check(variables.Count != 1, "wrong variables count");

            return new List<ExoAtom>() { new ExoAtom("$tier->" + variables[0].DeQuote()) };
        }
    }
}
