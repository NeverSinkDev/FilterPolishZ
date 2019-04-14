using FilterCore.Entry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterCore.Commands.EntryCommands
{
    public interface IEntryCommand
    {
        FilterEntry Target { get; set; }
        void Execute(int? strictness = null);
    }
}
