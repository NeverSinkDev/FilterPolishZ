using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            byte[] encodedText = Encoding.UTF8.GetBytes(input);

            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.OpenOrCreate, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }
    }
}
