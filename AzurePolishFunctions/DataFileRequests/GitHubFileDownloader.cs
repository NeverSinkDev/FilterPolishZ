using System.Linq;
using System.Net;

namespace AzurePolishFunctions.DataFileRequests
{
    public class GitHubFileDownloader
    {
        public string Download(string user, string repo)
        {
            var client = new WebClient();

            var zipFile = "tempRepoDownload" + user + "_" + repo + ".zip";
            var extractedPath = "tempRepoDownload" + user + "_" + repo + "_Unzipped";
            if (System.IO.File.Exists(zipFile)) System.IO.File.Delete(zipFile);
            if (System.IO.Directory.Exists(extractedPath)) System.IO.Directory.Delete(extractedPath, true);
            
            client.Headers.Add("user-agent", "Anything");
            client.DownloadFile("https://api.github.com/repos/" + user + "/" + repo + "/zipball", zipFile);
            
            System.IO.Compression.ZipFile.ExtractToDirectory(zipFile, extractedPath);
            extractedPath = System.IO.Directory.EnumerateDirectories(extractedPath).Single();

            return extractedPath;
        }
    }
}