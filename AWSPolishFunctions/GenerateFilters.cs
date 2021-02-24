using System;
using System.Collections.Generic;
using System.Text;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Threading.Tasks;
using System.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace AWSPolishFunctions
{
    public class GenerateFilters
    {
        public async Task<APIGatewayProxyResponse> FilterGenerationFunction(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            return new APIGatewayProxyResponse { Body = $"Hello + { apigProxyEvent.QueryStringParameters.First().Value.ToString() }", StatusCode = 200 };
        }
    }
}
