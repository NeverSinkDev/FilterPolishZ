using System;
using System.Collections.Generic;
using System.Text;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Threading.Tasks;
using System.Linq;
using AzurePolishFunctions.Procedures;
using System.Diagnostics;
using AWSPolishFunctions.Extension;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace AWSPolishFunctions
{
    public class GenerateFilters
    {
        public async Task<APIGatewayProxyResponse> FilterGenerationFunction(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var body = apigProxyEvent.Body;
            var w = new Stopwatch();
            w.Start();

            var routine = new MainGenerationRoutine();
            MainGenerationRoutine.Logging.SetCustomLoggingMessage((s) => { LambdaLogger.Log(s); });
            dynamic data = JsonConvert.DeserializeObject(body);
            string repoName = data.repoName;
            routine.Execute(body);

            Console.WriteLine("Done with MainGeneration Routine. Starting Extensions (AWS)");
            Console.WriteLine($"Variable Reponame is: {repoName}");

            MainGenerationRoutine.Publisher.UploadToFBS3("fb-beta-frontend", $@"/datafiles/filters/{repoName}");
            MainGenerationRoutine.Publisher.UploadToFBS3("fb-s3bucket-dev",  $@"/datafiles/filters/{repoName}");

            w.Stop();

            return new APIGatewayProxyResponse { Body = $"Generation Finished after {(int)(w.ElapsedMilliseconds / (float)1000)} seconds", StatusCode = 200 };
        }
    }
}
