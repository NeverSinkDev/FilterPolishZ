using System.Linq;
using FilterCore.Constants;
using FilterCore.Entry;
using FilterCore.FilterComponents.Tags;

namespace FilterCore.Commands.EntryCommands
{
    public class RemoveHighlightsThenHideEntryCommand : GenerationTag
    {
        public RemoveHighlightsThenHideEntryCommand(FilterEntry target) : base(target) {}

        public override void Execute(int? strictness = null, int? consoleStrictness = null)
        {
            if (!this.IsActiveOnStrictness(strictness.Value))
            {
                return;
            }

            // also hide on the next strictness after that
            if (this.IsActiveOnStrictness(strictness.Value - 1))
            {
                this.Target.Header.HeaderValue = "Hide";
            }
            
            // remove highlights (sound, beam, icon)
            FilterGenerationConfig.HighlightingIdents.ToList().ForEach(x => this.Target.Content.RemoveAll(x));
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