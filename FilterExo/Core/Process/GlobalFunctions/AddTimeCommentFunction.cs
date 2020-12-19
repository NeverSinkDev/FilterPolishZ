using System;
using System.Collections.Generic;
using System.Text;
using FilterExo.Core.PreProcess.Commands;
using FilterExo.Model;
using FilterPolishUtil.Extensions;

namespace FilterExo.Core.Process.GlobalFunctions
{
    public class AddTimeCommentFunction : IExoGlobalFunction
    {
        public string Name => "AddTimeComment";

        public List<ExoAtom> Execute(ExoBlock content, ExoExpressionCommand caller)
        {
            caller.Parent.SimpleComments.Add($"# {DateTime.Now}");
            caller.Parent.Type = FilterExoConfig.ExoFilterType.comment;

            return new List<ExoAtom>()
            {

            };
        }
    }
}
