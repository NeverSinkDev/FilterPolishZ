using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilterCore.Entry;
using FilterCore.FilterComponents.Tags;

namespace FilterCore.Commands.EntryCommands
{
    public class DisableEntryCommand : GenerationTag
    {
        public override void Execute(int? strictness = null, int? consoleStrictness = null)
        {
            if (consoleStrictness.HasValue)
            {
                var csTag = this.Target.Header.GenerationTags.Single(x => x is ConsoleStrictnessCommand);
                if (!csTag.IsActiveOnStrictness(consoleStrictness.Value))
                {
                    return;
                }
            }
            
            else if (!this.IsActiveOnStrictness(strictness.Value))
            {
                return;
            }
            
            this.Target.IsFrozen = true;
        }

        public override GenerationTag Clone()
        {
            return new DisableEntryCommand(this.Target)
            {
                Value = this.Value,
                Strictness = this.Strictness
            };
        }

        public DisableEntryCommand(FilterEntry target) : base(target) {}
    }
}
