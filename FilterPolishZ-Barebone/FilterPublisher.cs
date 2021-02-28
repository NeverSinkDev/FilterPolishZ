using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using AzurePolishFunctions.DataFileRequests;
using AzurePolishFunctions.Procedures;
using FilterCore;
using FilterPolishUtil;
using FilterPolishUtil.Model;
using FilterPolishZ.Util;
using LibGit2Sharp;
using Filter = FilterCore.Filter;
using Path = System.IO.Path;

namespace AzurePolishFunctions
{
    public class FilterPublisher
    {
        public Filter Filter { get; set; }
        public string League { get; set; }

        public string OutputFolder = string.Empty;
        public bool PublishPrice = true;

        public static string FilterDescription = "NeverSink's LOOTFILTER, in-depth, endgame+leveling 2in1, user-friendly, multiversion, updated and refined over 5 years. For more information and customization options, visit: www.filterblade.xyz";

        public FilterPublisher(Filter filter, string league)
        {
            this.Filter = filter;
            this.League = league;
        }
        
        public void Init(FileRequestResult dataRes)
        {
            // important for end-league scenarios, where no economy exists
            if (dataRes != FileRequestResult.Success)
            {
                PublishPrice = false;
            }

            var filterOutFolder = Path.GetTempPath() + "filterGenerationResult";
            if (Directory.Exists(filterOutFolder))
            {
                DeleteDirectory(filterOutFolder);
            }

            Directory.CreateDirectory(filterOutFolder);
            LoggingFacade.LogInfo($"Tempfolder prepared {filterOutFolder}");

            // create filter
            LoggingFacade.LogInfo($"Performing filter generation operations");
            var filterWriter = FilterWriter.WriteFilter(this.Filter, true, filterOutFolder + "/", Path.GetDirectoryName(MainGenerationRoutine.DataFiles.FilterStyleFilesPaths.First().Value) + "/");
            filterWriter.Wait();

            LoggingFacade.LogInfo($"Performing filter generation operations: DONE");
        }

        public void PublishToLadder()
        {
            LoggingFacade.LogInfo($"PoeUpload: starting");
            UploadToPoe(OutputFolder);
            LoggingFacade.LogInfo($"PoeUpload: done");
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
                    if (styleName == "Customsounds") styleName = "CustomSounds";
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

            var outpu = proc.StandardOutput.ReadToEnd();
            var errors = proc.StandardError.ReadToEnd();
            var isSuccess = proc.ExitCode == 0;

            if (!isSuccess)
            {
                Console.WriteLine(outpu);
                Console.WriteLine(errors);
            }
        }
        
        private void UploadToPoe(string filterFolder)
        {
            var token = Environment.GetEnvironmentVariable("PoeApiAccessToken");

            for (var i = 0; i < FilterGenerationConfig.FilterStrictnessApiIds[this.League].Count; i++)
            {
                var filterId = FilterGenerationConfig.FilterStrictnessApiIds[this.League][i];
                var filterPath = filterFolder + "\\NeverSink's filter - " + i + "-" + FilterGenerationConfig.FilterStrictnessLevels[i].ToUpper() + ".filter";
                var filterContent = FileWork.ReadFromFile(filterPath);
                this.UploadToPoe_Single(filterId, token, FilterDescription, filterContent);
            }
        }

        private void UploadToPoe_Single(string filterId, string accessToken, string descr, string filterContent)
        {
            var url = "https://www.pathofexile.com/api/item-filter/" + filterId + "?access_token=" + accessToken;

            var body = new
            {
                filter = filterContent,
                filterID = filterId,
                description = descr,
                version = FilterAccessFacade.GetInstance().PrimaryFilter.GetHeaderMetaData("VERSION")
            };
            
            LoggingFacade.LogInfo($"[PoeUpload] Sending request...");

            var resp = FileDownloader.StaticHttpClient.PostAsJsonAsync(url, body);
            resp.Wait();
            if (resp.Result.StatusCode != HttpStatusCode.OK)
            {
                LoggingFacade.LogError($"[PoeUpload] Error: " + resp.Result.StatusCode);
            }
            else
            {
                LoggingFacade.LogInfo($"[PoeUpload] Success");
            }
        }
    }
}