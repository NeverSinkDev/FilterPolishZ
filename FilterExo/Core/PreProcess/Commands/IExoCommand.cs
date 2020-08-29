using FilterExo.Model;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace FilterExo.Core.PreProcess.Commands
{
    public interface IExoCommand
    {
        ExoBlock Parent { get; set; }
        string Name { get; }
        IExoCommandType Type { get; }
        string SerializeDebug();
    }

    public static class EExoCommand
    {
        public static void SetParent(this IExoCommand me, ExoBlock parent)
        {
            me.Parent = parent;
        }
    }

    public enum IExoCommandType
    {
        scope,
        execution,
        filter
    }
}
