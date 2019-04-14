using System.Linq;
using FilterCore.Constants;
using FilterCore.Entry;
using FilterCore.FilterComponents.Tags;

namespace FilterCore.Commands.EntryCommands
{
    public class RemoveHighlightsEntryCommand : GenerationTag
    {
        public RemoveHighlightsEntryCommand(FilterEntry target) : base(target) {}

        public override void Execute(int? strictness = null)
        {
            if (!this.IsActiveOnStrictness(strictness.Value))
            {
                return;
            }
            
            FilterConstants.HighlightingIdents.ToList().ForEach(x => this.Target.Content.RemoveAll(x));
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