using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FilterEconomy.Request
{
    public class RestRequest
    {
        public string Path { get; set; }

        public RestRequest(string path)
        {
            this.Path = path;
        }

        public async Task<string> ExecuteAsync()
        {
            return await this.PerformAsyncRequest(this.Path);
        }

        public string Execute()
        {
            return PerformRequest(this.Path);
        }

        private static string PerformRequest(string url)
        {
//            using (WebClient wc = new WebClient() )
//            {
//                wc.Proxy=null;
//                return wc.DownloadString(url);
//            }
            
            return new WebClient().DownloadString(url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            // request.ContentType = contentType;
            request.Method = WebRequestMethods.Http.Get;
            request.Timeout = 20000;
            WebResponse response = request.GetResponse(); // todo: 

            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader sr = new StreamReader(responseStream))
            {
                string strContent = sr.ReadToEnd();
                return strContent;
            }
        }

        private async Task<string> PerformAsyncRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            // request.ContentType = contentType;
            request.Method = WebRequestMethods.Http.Get;
            request.Timeout = 20000;
            request.Proxy = null;
            WebResponse response = await request.GetResponseAsync();

            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader sr = new StreamReader(responseStream))
            {
                string strContent = sr.ReadToEnd();
                return strContent;
            }
        }
    }
}
