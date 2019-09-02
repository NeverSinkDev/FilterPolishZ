using System.Collections;
using System.Collections.Generic;
using FilterCore.Entry;

namespace FilterCore.Commands.EntryCommands
{
    public interface IEntryGenerationCommand
    {
        IEnumerable<IFilterEntry> NewEntries { get; set; }
    }
}