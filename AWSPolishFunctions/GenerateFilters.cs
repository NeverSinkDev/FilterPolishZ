using System;
using System.Collections.Generic;
using System.Text;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Threading.Tasks;
using System.Linq;

namespace AWSPolishFunctions
{
    public class GenerateFilters
    {
        public async Task<APIGatewayProxyResponse> FilterGenerationFunction(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            return new APIGatewayProxyResponse { Body = $"Hello + { apigProxyEvent.QueryStringParameters.First().Value.ToString() }" };
        }
    }
}
