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
            if (Strictness >= 0)
            {
                return $"%{Value}{Strictness}";
            }
            else
            {
                return string.Empty;
            }
        }

        public bool IsStrictnessCommand()
        {
            return this.Strictness != -1;
        }

        public abstract void Execute(int? strictness = null);

        protected bool IsActiveOnStrictness(int strictness)
        {
            // tag = 1 means: skip if the strictness is 1 or lower
            if (this.Strictness >= strictness)
            {
                return false;
            }

            return true;
        }

        public abstract GenerationTag Clone();
    }
}
