using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilterCore.Entry;
using FilterCore.FilterComponents.Tags;

namespace FilterCore.Commands.EntryCommands
{
    public class HideEntryCommand : GenerationTag
    {
        public override void Execute(int? strictness = null, int? consoleStrictness = null)
        {
            if (!this.IsActiveOnStrictness(strictness.Value, consoleStrictness))
            {
                return;
            }
            
            this.Target.Header.HeaderValue = "Hide";
        }
        
        public override GenerationTag Clone()
        {
            return new DisableEntryCommand(this.Target)
            {
                Value = this.Value,
                Strictness = this.Strictness
            };
        }

        public HideEntryCommand(FilterEntry target) : base(target) {}
    }
}
