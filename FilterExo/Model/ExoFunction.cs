using FilterExo.Core.PreProcess.Commands;
using FilterPolishUtil;
using FilterPolishUtil.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using FilterExo.Core.Process.GlobalFunctions;

namespace FilterExo.Model
{
    public enum ExoFunctionType
    {
        userdefined,
        global
    }

    public class ExoFunction
    {
        public ExoFunctionType Type = ExoFunctionType.userdefined;

        // global function specific params
        public IExoGlobalFunction GlobalFunctionLink;

        // general parameters
        public string Name;
        public ExoBlock Content;
        public List<string> Variables = new List<string>();

        public IEnumerable<List<ExoAtom>> Execute(Branch<ExoAtom> atom, ExoExpressionCommand caller)
        {
            TraceUtility.Check(atom.Content != null, "called function child has content!");

            // refactor this! currently cleans variables from previous runs
            foreach (var item in Variables)
            {
                Content.Variables.Remove(item);
            }

            var splitChildren = atom.Leaves.SplitDivide(x => x.Content?.GetRawValue() == ",");
            if (splitChildren.Count == 1 && splitChildren[0].Count == 0)
            {
                splitChildren.Clear();
            }

            TraceUtility.Check(splitChildren.Count != Variables.Count, "function call has unequal children definition");

            // adds variables int function
            for (int i = 0; i < splitChildren.Count; i++)
            {
                var vari = splitChildren[i];
                var name = Variables[i];

                var flattened = ExoExpressionCommand.FlattenBranch(vari);
                Content.Variables.Add(name, new ExoAtom(flattened));
            }

            switch (Type)
            {
                // normal execution
                case ExoFunctionType.userdefined:
                    foreach (var item in Content.Commands)
                    {
                        yield return item.ResolveExpression();
                    }
                    break;
                // global function execution
                case ExoFunctionType.global:
                    yield return GlobalFunctionLink.Execute(Content, caller);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
