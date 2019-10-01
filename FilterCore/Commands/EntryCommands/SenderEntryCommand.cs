using System.Linq;
using FilterCore.Constants;
using FilterCore.Entry;
using FilterCore.FilterComponents.Tags;
using FilterDomain.LineStrategy;

namespace FilterCore.Commands.EntryCommands
{
    public class SenderEntryCommand : GenerationTag
    {
        public string TypeValue { get; set; }
        
        public SenderEntryCommand(FilterEntry target) : base(target) {}

        public override void Execute(int? strictness = null, int? consoleStrictness = null)
        {
        }
        
        public override GenerationTag Clone()
        {
            return new SenderEntryCommand(this.Target)
            {
                Value = this.Value,
                Strictness = this.Strictness,
                TypeValue = this.TypeValue
            };
        }
    }
}