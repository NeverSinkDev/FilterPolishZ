using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilterPolishUtil.Model;

namespace FilterPolishUtil
{
    public static class FileWork
    {
        public static List<string> ReadLinesFromFile(string adress)
        {
            try
            {   // Open the text file using a stream reader.

                var logFile = File.ReadAllLines(adress);
                var logList = new List<string>(logFile);
                return logList;
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static string ReadFromFile(string adress)
        {
            try
            {   // Open the text file using a stream reader.
                var logFile = File.ReadAllText(adress);
                return logFile;
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                LoggingFacade.LogError("File cound not be read. Path: " + adress + ". Error: " + e.Message);
                return null;
            }
        }

        public static async Task WriteTextAsync(string filePath, List<string> input)
        {
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            
            var mergedText = string.Join(System.Environment.NewLine, input);
            byte[] encodedText = Encoding.UTF8.GetBytes(mergedText);

            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.OpenOrCreate, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }

        public static async Task WriteTextAsync(string filePath, string input)
        {
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
            byte[] encodedText = Encoding.UTF8.GetBytes(input);

            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.OpenOrCreate, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }

        public static void WriteText(string filePath, List<string> input)
        {
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

            var mergedText = string.Join(System.Environment.NewLine, input);
            byte[] encodedText = Encoding.UTF8.GetBytes(mergedText);

            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.OpenOrCreate, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                sourceStream.Write(encodedText, 0, encodedText.Length);
            };
        }

        public static void WriteText(string filePath, string input)
        {
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
            byte[] encodedText = Encoding.UTF8.GetBytes(input);

            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.OpenOrCreate, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: false))
            {
                sourceStream.Write(encodedText, 0, encodedText.Length);
            };
        }

        public static void ZipDirectory(this ZipArchive zipArchive, string srcDir, Func<string, bool> fileFilter, Func<string, bool> folderFilter, string rootDir = "")
        {
            if (!Directory.Exists(srcDir)) throw new Exception("source directory for zipping doesn't exit");
            var dir = new DirectoryInfo(srcDir);

            dir.GetFiles().ToList().ForEach((file) => {
                if (fileFilter(file.Name))
                {
                    zipArchive.CreateEntryFromFile(file.FullName, string.IsNullOrEmpty(rootDir) ? file.Name : $@"{rootDir}\{file.Name}");
                }
            });

            dir.GetDirectories().ToList().ForEach((directory) => {
                if (folderFilter(directory.Name))
                {
                    zipArchive.ZipDirectory(directory.FullName, fileFilter, folderFilter, string.IsNullOrEmpty(rootDir) ? $@"{directory.Name}" : $@"{rootDir}\{directory.Name}");
                }
            });
        }
    }
}
