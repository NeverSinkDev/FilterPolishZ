using LibGit2Sharp;
using System.IO;
using System.Linq;
using System.Net;

namespace AzurePolishFunctions.DataFileRequests
{
    public class GitHubFileDownloader
    {
        public string Download(string user, string repo)
        {   
            var zipFile = Path.GetTempPath() + "\\" + "tempRepoDownload" + user + "_" + repo + ".zip";
            var extractedPath = Path.GetTempPath() + "\\" + "tempRepoDownload" + user + "_" + repo + "_Unzipped";
            if (System.IO.File.Exists(zipFile)) System.IO.File.Delete(zipFile);
            if (System.IO.Directory.Exists(extractedPath)) FilterPublisher.DeleteDirectory(extractedPath);

            System.IO.Directory.CreateDirectory(extractedPath);

            Repository.Clone("https://github.com/NeverSinkDev/" + repo + ".git", extractedPath);
            // FilterPublisher.RunCommand(extractedPath, "git", "clone https://github.com/NeverSinkDev/" + repo + ".git");

            return extractedPath;
        }
    }
}