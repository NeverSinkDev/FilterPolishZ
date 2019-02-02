using FilterCore.Entry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterCore.Commands.EntryCommands
{
    public interface IEntryCommand : IFilterCommand
    {
        FilterEntry Target { get; set; }
    }
}
