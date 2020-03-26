using System.Collections.Generic;
using System.Linq;
using FilterCore.Entry;
using FilterCore.FilterComponents.Tags;
using FilterDomain.LineStrategy;

namespace FilterCore.Commands.EntryCommands
{
    public class RaresUpEntryCommand : GenerationTag, IEntryGenerationCommand
    {
        public RaresUpEntryCommand(FilterEntry target) : base(target) {}
        public IEnumerable<IFilterEntry> NewEntries { get; set; }

        public override void Execute(int? strictness = null, int? consoleStrictness = null)
        {
            var newEntry = this.Target.Clone();
            newEntry.Header.TierTags.AppendUpSuffixToTierTag();

            var textLine = newEntry.Content.Content["SetTextColor"].Single();
            var levelLine = newEntry.Content.Content["ItemLevel"].Single();

            if (textLine.Value is ColorValueContainer color)
            {
                color.R = 255;
                color.G = 190;
                color.B = 0;
                color.O = 255;
            }

            if (levelLine.Value is NumericValueContainer val)
            {
                val.Value = "75";
            }
            
            this.NewEntries = new List<IFilterEntry> { newEntry };
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