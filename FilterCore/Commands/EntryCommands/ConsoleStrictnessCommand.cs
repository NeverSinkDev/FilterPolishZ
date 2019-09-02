using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilterCore.Entry;
using FilterCore.FilterComponents.Tags;

namespace FilterCore.Commands.EntryCommands
{
    public class ConsoleStrictnessCommand : GenerationTag
    {
        public override void Execute(int? strictness = null, int? consoleStrictness = null)
        {
            // This command doesnt actually do anything by itself.
            // It is just a "marker" for other commands to know which strictness to compare it to.
        }

        public override GenerationTag Clone()
        {
            return new ConsoleStrictnessCommand(this.Target)
            {
                Value = this.Value,
                Strictness = this.Strictness
            };
        }

        public ConsoleStrictnessCommand(FilterEntry target) : base(target) {}
    }
}
