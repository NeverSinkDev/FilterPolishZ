using System;
using System.Diagnostics;
using System.IO;
using FilterCore;
using FilterPolishZ.Util;

namespace AzurePolishFunctions
{
    public class FilterPublisher
    {
        public Filter Filter { get; set; }
        
        public FilterPublisher(Filter filter)
        {
            this.Filter = filter;
        }
        
        public void Run()
        {
            var repoName = "NeverSink-EconomyUpdated-Filter";
            var filterOutFolder = Path.GetTempPath() + "/filterGenerationResult";
            var repoFolder = filterOutFolder + "\\" + repoName;
            if (!Directory.Exists(filterOutFolder)) Directory.CreateDirectory(filterOutFolder);
            
            // clone
            if (Directory.Exists(repoName)) DeleteDirectory(repoFolder);
            RunCommand(filterOutFolder, "git", "clone https://github.com/NeverSinkDev/" + repoName + ".git");
            
            // create filter
            FilterWriter.WriteFilter(this.Filter, false, repoFolder, null);
            File.WriteAllText(repoFolder + "\\testCommit", "testContent");

            // commit
            RunCommand(repoFolder,  "git",  "add -A");
            RunCommand(repoFolder,  "git",  "commit -m \"automated economy update\"");
            
            // push + login
            RunCommand(repoFolder,  "git",  "config --global user.name \"AutomatedEconomyUpdate\"");
//            var gitToken = File.ReadAllText(filterOutFolder + "\\githubPAT.txt");
//            RunCommand(repoFolder,  "ssh",  "-i " + filterOutFolder + "\\githubPAT.txt user@github.com", "yes");
            RunCommand(repoFolder,  "git",  "push https://github.com/NeverSinkDev/" + repoName + ".git master");
            
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

        private static void RunCommand(string workDir, string cmd, string param, string input = null)
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