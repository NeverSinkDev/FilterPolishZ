using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilterCore.Commands.EntryCommands;
using FilterCore.Entry;

namespace FilterCore.FilterComponents.Tags
{
    public abstract class GenerationTag : IEntryCommand
    {
        public string Value { get; set; }
        public short Strictness { get; set; } = -1;
        public FilterEntry Target { get; set; }

        protected GenerationTag(FilterEntry target)
        {
            this.Target = target;
        }

        public string Serialize()
        {
            if (this.IsStrictnessCommand())
            {
                return $"%{Value}{Strictness}";
            }

            if (this is ReceiverEntryCommand rec)
            {
                return $"%{this.Value}{rec.TypeValue}";
            }
            if (this is SenderEntryCommand sen)
            {
                return $"%{this.Value}{sen.TypeValue}";
            }

            return $"%{Value}";
        }

        public bool IsStrictnessCommand()
        {
            return this.Strictness != -1;
        }

        public abstract void Execute(int? strictness = null, int? consoleStrictness = null);

        protected bool IsActiveOnStrictness(int strictness)
        {
            // tag = 1 means: skip if the strictness is 1 or lower
            if (this.Strictness >= strictness)
            {
                return false;
            }

            return true;
        }
        
        // use this function instead of the 1 param overload when the tag/command should work with console strictnesses
        protected bool IsActiveOnStrictness(int strictness, int? consoleStrictness)
        {
            // we're not generating console versions -> use normal
            if (!consoleStrictness.HasValue) return this.IsActiveOnStrictness(strictness);
            
            // no console strictness exception present -> use normal handling
            var csTag = this.Target.Header.GenerationTags.FirstOrDefault(x => x is ConsoleStrictnessCommand);
            if (csTag == null) return this.IsActiveOnStrictness(strictness);
            
            // console strictness exception -> handle via console tag command value + console strictness param
            return csTag.IsActiveOnStrictness(consoleStrictness.Value);
        }

        public abstract GenerationTag Clone();
    }
}
