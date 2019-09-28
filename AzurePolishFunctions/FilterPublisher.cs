using System;
using System.Diagnostics;
using System.IO;
using FilterCore;
using FilterPolishZ.Util;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace AzurePolishFunctions
{
    public class FilterPublisher
    {
        public FilterCore.Filter Filter { get; set; }
        
        public FilterPublisher(FilterCore.Filter filter)
        {
            this.Filter = filter;
        }
        
        public void Run()
        {
            var repoName = "NeverSink-EconomyUpdated-Filter";
            var filterOutFolder = Path.GetTempPath() + "filterGenerationResult";
            var repoFolder = filterOutFolder + "\\" + repoName;
            if (!Directory.Exists(filterOutFolder)) Directory.CreateDirectory(filterOutFolder);
            
            // clone
            if (Directory.Exists(repoFolder)) DeleteDirectory(repoFolder);
            // RunCommand(filterOutFolder, "git", "clone https://github.com/NeverSinkDev/" + repoName + ".git");

            Repository.Clone("https://github.com/NeverSinkDev/" + repoName + ".git", repoFolder);

            // create filter
            FilterWriter.WriteFilter(this.Filter, false, repoFolder, null);

            var gitToken = Environment.GetEnvironmentVariable("githubPAT", EnvironmentVariableTarget.Process);

            Signature author = new Signature("FilterBlade", "@neversinkdev", DateTime.Now);
            Signature committer = author;

            using (var repo = new Repository(repoFolder))
            {
                Commands.Stage(repo, "*");
                Commit commit = repo.Commit("automated economy update " + DateTime.Today.ToString(), author, committer);

                LibGit2Sharp.PushOptions options = new LibGit2Sharp.PushOptions();
                options.CredentialsProvider = new CredentialsHandler(
                    (url, usernameFromUrl, types) =>
                        new UsernamePasswordCredentials()
                        {
                            Username = "FilterBladeTeam",
                            Password = gitToken
                        });
                repo.Network.Push(repo.Branches["master"], options);
            }

            // Commit to the repository


            // commit
            // RunCommand(repoFolder,  "git",  "add -A");
            // RunCommand(repoFolder,  "git",  "commit -m \"automated economy update\"");
            // push + login
            // RunCommand(repoFolder,  "git",  "config --global user.name \"AutomatedEconomyUpdate\"");
//            RunCommand(repoFolder,  "ssh",  "-i " + filterOutFolder + "\\githubPAT.txt jevjaku@github.com", "yes");
            // RunCommand(repoFolder,  "git",  "push https://" + gitToken + "@github.com/NeverSinkDev/" + repoName + ".git master");
            
            // cleanUp
            DeleteDirectory(repoFolder);
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