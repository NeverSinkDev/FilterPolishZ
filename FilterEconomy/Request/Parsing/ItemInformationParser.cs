using FilterEconomy.Model.ItemInformationData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterEconomy.Request.Parsing
{
    public class ItemInformationParser
    {
        public static IEnumerable<ItemInformationData> ParseItemInformationString(string input)
        {
            dynamic jsonObj = JsonConvert.DeserializeObject<dynamic>(input, new JsonSerializerSettings() { CheckAdditionalContent = true });
            foreach (JObject job in jsonObj.lines)
            {
                ItemInformationData nItem = new ItemInformationData();
                JsonConvert.PopulateObject(job.ToString(), nItem);
                yield return nItem;
            }
        }

        public static Dictionary<string, List<ItemInformationData>> CreateOverviewDictionary(IEnumerable<ItemInformationData> input)
        {
            var result = new Dictionary<string, List<ItemInformationData>>();

            foreach (var item in input)
            {
                var key = item.Name;
                if (!result.ContainsKey(key))
                {
                    result.Add(key, new List<ItemInformationData>());
                }

                result[key].Add(item);
            }

            return result;
        }
    }
}
