using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilterEconomy.Facades;
using FilterEconomy.Processor;

namespace FilterEconomy.Model
{
    public class TieringChange
    {
        // 1 => moved up
        // 0 => stayed same
        // -1 => lost tiers
        public string Category { get; set; }
        public int Change { get; set; } = 0;
        public string BaseType { get; set; }

        public List<TieringChangeInnerItemInformaiton> InnerInformation { get; set; } = new List<TieringChangeInnerItemInformaiton>();
        public string OldTier { get; set; }
        public string NewTier { get; set; }
        public string Reason { get; set; }
        public float Confidence { get; set; }

        public static TieringChange FromTieringCommand(TieringCommand item)
        {
            TieringChange result = new TieringChange();

            result.BaseType = item.BaseType;
            result.OldTier = item.NewTier;
            result.NewTier = item.NewTier;
            result.Reason = item.AppliedRule;
            result.Confidence = item.Confidence;
            result.Category = item.Group;

            result.Change = TranslateChange(item);

            if (!item.Group.ToLower().Contains("rare"))
            {
                var items = ItemInformationFacade.GetInstance()[item.Group, item.BaseType];
                if (items != null)
                {
                    result.InnerInformation =
                        items.GroupBy(x => x.Name)
                        .Select(x => x.First())
                        .Select(x => new TieringChangeInnerItemInformaiton() { Name = x.Name, Icon = "", Aspects = null })
                        .ToList();
                }
            }

            return result;
        }

        private static int TranslateChange(TieringCommand item)
        {
            if (item.OldTier == item.NewTier)
            {
                return 0;
            }

            return 1;
        }
    }

    public class TieringChangeInnerItemInformaiton
    {
        public string Icon;
        public string Name;
        public List<string> Aspects;
    }
}
