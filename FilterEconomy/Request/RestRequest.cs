using FilterPolishUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            return new WebClient() { Encoding = Encoding.UTF8 }.DownloadString(url);
        }

        private static string PerformRequestHTTPREQ(string url)
        {
            var client = FileDownloader.StaticHttpClient;

            var dlTask = client.GetStringAsync(url);
            dlTask.Wait();
            return dlTask.Result;
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
