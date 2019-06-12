using System;
using System.Collections.Generic;
using System.IO;
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
            // this client somehow stopped working. on one hand it suddenly needed a user agent specified, on the other it
            // crashed because of endless redirects.
//            var client = new WebClient();
//            client.Headers.Add("user-agent", "any");  
//            var fullString = client.DownloadString(DataFileUrl);
            
            var client = (HttpWebRequest) WebRequest.Create(DataFileUrl);
            client.UserAgent = "any";
            client.CookieContainer = new CookieContainer();

            var fullString = "";
            var reader = new StreamReader(client.GetResponse().GetResponseStream()); 
            while (!reader.EndOfStream)
            {
                fullString += (reader.ReadLine()) + "\n";
            }

            string[] stats = null;
            var baseTypeData = new Dictionary<string, Dictionary<string, string>>();

            var isFirstLine = true;
            foreach (var line in fullString.Split('\n'))
            {
                if (string.IsNullOrEmpty(line)) continue;
                
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

        private static string GetResponseFromUrl(string dataFileUrl)
        {
            string fullString;
            try
            {
                var myUri = new Uri(dataFileUrl);
                // Create a 'HttpWebRequest' object for the specified url. 
                var myHttpWebRequest = (HttpWebRequest)WebRequest.Create(myUri);
                // Set the user agent as if we were a web browser
                myHttpWebRequest.UserAgent = @"Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.0.4) Gecko/20060508 Firefox/1.5.0.4";
                myHttpWebRequest.AllowAutoRedirect = false;

                var myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                var stream = myHttpWebResponse.GetResponseStream();
                var reader = new StreamReader(stream);
                fullString = reader.ReadToEnd();
                // Release resources of response object.

                if ((int)myHttpWebResponse.StatusCode >= 300 && (int)myHttpWebResponse.StatusCode <= 399)
                {
                    string uriString = myHttpWebResponse.Headers["Location"];
                    Console.WriteLine("Redirect to " + uriString ?? "NULL");

                    fullString = GetResponseFromUrl(uriString);
                }

                myHttpWebResponse.Close();

            }
            catch (WebException ex)
            {
                using (var sr = new StreamReader(ex.Response.GetResponseStream()))
                    fullString = sr.ReadToEnd();
            }

            return fullString;
        }
    }
}