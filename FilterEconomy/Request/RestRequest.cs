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
            try
            {
                return await this.PerformAsyncRequest(this.Path).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private static string PerformRequest(string url)
        {  
            return new WebClient() { Encoding = Encoding.UTF8 }.DownloadString(url);
        }

        private async Task<string> PerformAsyncRequest(string url)
        {
            using (var client = new HttpClient())
            {
                var req = new HttpRequestMessage(HttpMethod.Get, url);
                var response = await client.SendAsync(req).ConfigureAwait(false);
                var responseX = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return responseX;
            }
        }
    }
}
