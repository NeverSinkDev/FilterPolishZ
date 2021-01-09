using System;
using System.Collections.Generic;
using System.Text;
using FilterEconomy.Model;
using FilterPolishUtil;

namespace FilterEconomy.Request.Parsing
{
    public static class NinjaParsingFixes
    {
        public static NinjaItem Process(string section, NinjaItem item)
        {
            switch (section)
            {
                case "gems":
                    return ProcessGem(item);
                default:
                    return item;
            }
        }

        private static NinjaItem ProcessGem(NinjaItem item)
        {
            List<string> qualityTypes = new List<string>(){ "Divergent", "Phantasmal", "Anomalous" };
            var quality = item.Name.ContainsAny(qualityTypes);

            if (quality != string.Empty)
            {
                item.Name = item.Name.Replace(quality, "").Trim();
                item.EQualityType = quality;
            }
            else
            {
                item.EQualityType = string.Empty;
            }

            return item;
        }
    }
}
