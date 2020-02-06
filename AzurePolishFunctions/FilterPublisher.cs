using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using AzurePolishFunctions.DataFileRequests;
using FilterPolishUtil.Model;
using FilterPolishZ.Util;
using LibGit2Sharp;
using Filter = FilterCore.Filter;

namespace AzurePolishFunctions
{
    public class FilterPublisher
    {
        public Filter Filter { get; set; }
        public string RepoName {get; set;}

        public bool PublishPrice = true;
        
        public FilterPublisher(Filter filter, string repoName)
        {
            this.RepoName = repoName;
            this.Filter = filter;
        }
        
        public void Run(FileRequestResult dataRes)
        {
            if (dataRes != FileRequestResult.Success)
            {
                PublishPrice = false;
            }

            var filterOutFolder = Path.GetTempPath() + "filterGenerationResult";
            var repoFolder = filterOutFolder + "\\" + RepoName;

            LoggingFacade.LogInfo($"Tempfolder prepared");

            if (!Directory.Exists(filterOutFolder)) Directory.CreateDirectory(filterOutFolder);
            
            // clone/pull the repo. after that, we will edit these existing files by generating the new versions
            // and push the update as the actual small changes.
            if (Directory.Exists(repoFolder))
            {
                LoggingFacade.LogInfo($"Repo folder existing... pulling");
                RunCommand(filterOutFolder, "git", "branch --set-upstream-to=origin/master master");
                using (var repo = new Repository(repoFolder))
                {
                    var options = new PullOptions();
                    var author = Environment.GetEnvironmentVariable("author", EnvironmentVariableTarget.Process) ?? "FilterPolishZ";
                    var email = Environment.GetEnvironmentVariable("email", EnvironmentVariableTarget.Process) ?? "FilterPolishZ";
                    Commands.Pull(repo, new Signature(author, email, DateTimeOffset.Now), options);
                    LoggingFacade.LogInfo($"Pulling done");
                }
            }
            else
            {
                LoggingFacade.LogInfo($"Repo folder not existing... cloning");
                Repository.Clone("https://github.com/NeverSinkDev/" + RepoName + ".git", repoFolder);
                LoggingFacade.LogInfo($"Cloning done!");
            }

            // create filter
            LoggingFacade.LogInfo($"Performing filter generation operations");
            FilterWriter.WriteFilter(this.Filter, true, repoFolder + "\\", Path.GetDirectoryName(GenerateFilters.DataFiles.FilterStyleFilesPaths.First().Value) + "\\");
            LoggingFacade.LogInfo($"Performing filter generation operations: DONE");

            LoggingFacade.LogInfo($"Starting publishing!");

            PushToFTP("www", repoFolder, "NeverSink_AutoEcoUpdate_" + GenerateFilters.DataFiles.LeagueType);
            LoggingFacade.LogInfo($"Publishing to filterblade: done");

            PushToFTP("beta", repoFolder, "NeverSink_AutoEcoUpdate_" + GenerateFilters.DataFiles.LeagueType);
            LoggingFacade.LogInfo($"Publishing to filterblade-beta: done");

            PushToGit(repoFolder, PublishPrice);
            LoggingFacade.LogInfo($"Publishing to GitHub: done");

            // no cleanUp -> we keep this folder here and just pull/push whenever we generate new filters
        }

        private static void PushToFTP(string variant, string localFolder, string filterName)
        {
            var ftpHost = "ftp://ftp.cluster023.hosting.ovh.net/" + variant + "/datafiles/filters/" + filterName;
            
            using (var client = new WebClient())
            {
                client.Credentials = new NetworkCredential(Environment.GetEnvironmentVariable("FbFtpLoginName"), Environment.GetEnvironmentVariable("FbFtpLoginPw"));
                MakeDir(ftpHost, client.Credentials);
                
                foreach (var styleFolder in Directory.EnumerateDirectories(localFolder))
                {
                    var styleName = Path.GetFileName(styleFolder).Replace("(STYLE) ", "");
                    styleName = styleName.Substring(0, 1).ToUpper() + styleName.Substring(1).ToLower();
                    MakeDir(ftpHost + "/" + styleName, client.Credentials);
                    
                    foreach (var file in Directory.EnumerateFiles(styleFolder))
                    {
                        var relativePath = Path.GetRelativePath(localFolder, file);
                        relativePath = relativePath.Replace(Path.GetFileName(styleFolder), styleName);
                        UploadFile(client, ftpHost + "/" + relativePath, file);
                    }
                }

                ftpHost += "/Normal";
                MakeDir(ftpHost, client.Credentials);
                
                foreach (var file in Directory.EnumerateFiles(localFolder))
                {
                    var relativePath = Path.GetRelativePath(localFolder, file);
                    UploadFile(client, ftpHost + "/" + relativePath, file);
                }
            }

            void UploadFile(WebClient client, string path, string file)
            {
                var request = WebRequest.Create(path);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.Credentials = client.Credentials;
                try
                {
                    using (var resp = (FtpWebResponse) request.GetResponse())
                    {
                        Console.WriteLine(resp.StatusCode);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("File already deleted");
                }
                
                client.UploadFile(path, WebRequestMethods.Ftp.UploadFile, file);
            }

            void MakeDir(string dir, ICredentials credential)
            {
                var request = WebRequest.Create(dir);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = credential;
                try
                {
                    using (var resp = (FtpWebResponse) request.GetResponse())
                    {
                        Console.WriteLine(resp.StatusCode);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Folder already exists");
                }
            }
        }

        private static void PushToGit(string repoFolder, bool publishPrice)
        {
            var author = Environment.GetEnvironmentVariable("author", EnvironmentVariableTarget.Process) ?? "FilterPolishZ";
            var email = Environment.GetEnvironmentVariable("email", EnvironmentVariableTarget.Process) ?? "FilterPolishZ";
            var gitToken = Environment.GetEnvironmentVariable("githubPAT", EnvironmentVariableTarget.Process);

            Signature sig = new Signature(author, email, DateTime.Now);
            Signature committer = sig;

            using (var repo = new Repository(repoFolder))
            {
                Commands.Stage(repo, "*");

                string priceInformation = "";

                if (publishPrice)
                {
                    priceInformation = " ex:c " + FilterPolishUtil.FilterPolishConfig.ExaltedOrbPrice;
                }
                else
                {
                    priceInformation = " no temp league eco data!";
                }

                Commit commit = repo.Commit("automated economy update " + DateTime.Today.ToString("MM/dd/yyyy") + priceInformation , sig, committer);

                PushOptions options = new PushOptions();
                options.CredentialsProvider = (url, usernameFromUrl, types) =>
                    new UsernamePasswordCredentials
                    {
                        Username = author,
                        Password = gitToken
                    };
                repo.Network.Push(repo.Branches["master"], options);
            }
        }

        // this special function is required because the normal delete function does not have enough access rights to delete git files
        public static void DeleteDirectory(string targetDir)
        {
            File.SetAttributes(targetDir, FileAttributes.Normal);

            foreach (var file in Directory.GetFiles(targetDir))
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (var dir in Directory.GetDirectories(targetDir))
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        }

        public static void RunCommand(string workDir, string cmd, string param, string input = null)
        {
            var proc = Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = workDir,
                FileName = cmd,
                Arguments = param,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            
//            if (!string.IsNullOrEmpty(input) && !proc.HasExited)
//            {
//                proc.WaitForInputIdle();
//                proc.StandardInput.WriteLine(input);
//            }

            var outpu = proc.StandardOutput.ReadToEnd();
            var errors = proc.StandardError.ReadToEnd();
            var isSuccess = proc.ExitCode == 0;

            if (!isSuccess)
            {
                Console.WriteLine(outpu);
                Console.WriteLine(errors);
            }
        }
    }
}