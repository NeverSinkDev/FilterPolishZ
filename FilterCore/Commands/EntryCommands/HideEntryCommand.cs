using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilterCore.Entry;

namespace FilterCore.Commands.EntryCommands
{
    public class HideEntryCommand : IEntryCommand
    {
        public FilterEntry Target { get; set; }

        public void Execute()
        {
            this.Target.Header.HeaderValue = "Hide";
        }
    }
}
