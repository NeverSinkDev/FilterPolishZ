using System.Collections.Generic;
using System.Net;

namespace FilterCore.Constants
{
    public static class BaseTypeDataProvider
    {
        private const string DataFileUrl = "https://www.filterblade.xyz/datafiles/other/BasetypeStorage.csv";
        private static Dictionary<string, Dictionary<string, string>> data;
        public static Dictionary<string, Dictionary<string, string>> BaseTypeData => data ?? (data = LoadData());

        private static Dictionary<string, Dictionary<string, string>> LoadData()
        {
            var client = new WebClient();
            var fullString = client.DownloadString(DataFileUrl);

            string[] stats = null;
            var baseTypeData = new Dictionary<string, Dictionary<string, string>>();
            
            var isFirstLine = true;
            foreach (var line in fullString.Split('\n'))
            {
                var words = line.Split(',');
                
                if (isFirstLine)
                {
                    stats = words;
                    isFirstLine = false;
                    continue;
                }

                string baseType = null;
                var statDic = new Dictionary<string, string>();
                for (var i = 0; i < words.Length; i++)
                {
                    var value = words[i];
                    var stat = stats[i];

                    if (stat == "BaseType") baseType = value;
                    
                    statDic.Add(stat, value);
                }
                
                baseTypeData.Add(baseType, statDic);
            }

            return baseTypeData;
        }
    }
}