using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using AzurePolishFunctions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AWSPolishFunctions.Extension
{
    public static class EFilterPublisher
    {
        private static readonly AmazonS3Client DbClient = new AmazonS3Client(RegionEndpoint.EUCentral1);

        public static async Task<AmazonWebServiceResponse> UploadToS3(string bucket, string name, string content, S3CannedACL accessPolicy = null)
        {
            var req = new PutObjectRequest();
            req.Key = name;
            req.BucketName = bucket;
            req.ContentBody = content;
            if (accessPolicy != null) req.CannedACL = accessPolicy;
            var res = await DbClient.PutObjectAsync(req);
            return res;
        }

        public static void UploadToFBS3(this FilterPublisher me, string serverFolderPrefix, string bucket)
        {
            UploadDirAsync(me.OutputFolder, serverFolderPrefix, bucket).Wait();
        }

        private static async Task UploadDirAsync(string directoryPath, string serverFolderPrefix, string existingBucketName)
        {
            try
            {
                var directoryTransferUtility =
                    new TransferUtility(DbClient);

                var request = new TransferUtilityUploadDirectoryRequest
                {
                    BucketName = existingBucketName,
                    Directory = directoryPath,
                    SearchOption = SearchOption.AllDirectories,
                    KeyPrefix = serverFolderPrefix,
                    CannedACL = S3CannedACL.PublicRead
                };

                await directoryTransferUtility.UploadDirectoryAsync(request);
                Console.WriteLine($"{directoryPath} uploaded to {serverFolderPrefix} on {existingBucketName}");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                        "Error encountered ***. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }
    }
}
