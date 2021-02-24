using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace FilterPolishUtil
{
    public class FileDownloader
    {
        public static HttpClient StaticHttpClient = new HttpClient();
    }

    public static class EHttpClient
    {
        public static async Task<HttpResponseMessage> PostAsJsonAsync<TModel>(this HttpClient client, string requestUrl, TModel model)
        {
            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(model);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            return await client.PostAsync(requestUrl, stringContent);
        }
    }
}
