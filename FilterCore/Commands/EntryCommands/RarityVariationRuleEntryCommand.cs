using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Animation;
using FilterCore.Entry;
using FilterCore.FilterComponents.Tags;
using FilterCore.Line;
using FilterDomain.LineStrategy;

namespace FilterCore.Commands.EntryCommands
{
    public class RarityVariationRuleEntryCommand : GenerationTag, IEntryGenerationCommand
    {
        public RarityVariationRuleEntryCommand(FilterEntry target) : base(target) {}
        public IEnumerable<IFilterEntry> NewEntries { get; set; }

        public override void Execute(int? strictness = null)
        {
            this.CheckTarget();
            
            var newEntry = this.Target.Clone();
            
            this.EditEntry(newEntry, 255, 255, 255, 255, "Normal");
            this.EditEntry(this.Target, 25, 95, 235, 255, "Magic");
            
            this.NewEntries = new List<IFilterEntry> { newEntry };
        }

        private void CheckTarget()
        {
            if (!this.Target.Content.Content.ContainsKey("SetTextColor"))
            {
                this.Target.Content.Content.Add("SetTextColor", new List<IFilterLine>
                {
                    new FilterLine<ColorValueContainer>
                    {
                        Ident = "SetTextColor",
                        Parent = this.Target,
                        Value = new ColorValueContainer()
                    }
                });
            }
            
            if (!this.Target.Content.Content.ContainsKey("Rarity"))
            {
                this.Target.Content.Content.Add("Rarity", new List<IFilterLine>
                {
                    new FilterLine<NumericValueContainer>
                    {
                        Ident = "Rarity",
                        Parent = this.Target,
                        Value = new NumericValueContainer()
                    }
                });
            }
        }

        private void EditEntry(IFilterEntry entry, short textR, short textG, short textB, short textO, string rarity)
        {
            var textLine = entry.Content.Content["SetTextColor"].Single();
            var rarityLine = entry.Content.Content["Rarity"].Single();

            if (textLine.Value is ColorValueContainer color)
            {
                color.R = textR;
                color.G = textG;
                color.B = textB;
                color.O = textO;
            }

            if (rarityLine.Value is NumericValueContainer rar)
            {
                rar.Value = rarity;
            }
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