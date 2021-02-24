using System;
using System.Collections.Generic;
using System.Text;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Threading.Tasks;
using System.Linq;
using AzurePolishFunctions.Procedures;
using System.Diagnostics;

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
            routine.Execute(body);

            w.Stop();

            return new APIGatewayProxyResponse { Body = $"Generation Finished after {(int)(w.ElapsedMilliseconds / (float)1000)} seconds", StatusCode = 200 };
        }
    }
}
