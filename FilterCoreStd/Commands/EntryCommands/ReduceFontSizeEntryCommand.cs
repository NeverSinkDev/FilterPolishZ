using System.Linq;
using FilterCore.Entry;
using FilterCore.FilterComponents.Tags;
using FilterDomain.LineStrategy;

namespace FilterCore.Commands.EntryCommands
{
    public class ReduceFontSizeEntryCommand : GenerationTag
    {
        public ReduceFontSizeEntryCommand(FilterEntry target) : base(target) {}

        public override void Execute(int? strictness = null, int? consoleStrictness = null)
        {
            if (!this.IsActiveOnStrictness(strictness.Value))
            {
                return;
            }
            
            var line = this.Target.Content.GetFirst("SetFontSize");
            if (line == null) return;
            var val = (VariableValueContainer) line.Value;
            val.Value.First().value = "36";
        }
        
        public override GenerationTag Clone()
        {
            return new DisableEntryCommand(this.Target)
            {
                Value = this.Value,
                Strictness = this.Strictness
            };
        }
    }
}