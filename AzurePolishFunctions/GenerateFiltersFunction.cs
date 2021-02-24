using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzurePolishFunctions.Procedures;

namespace AzurePolishFunctions
{
    public static class GenerateFiltersFunction
    {
        [FunctionName("GenerateFiltersFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var data = new StreamReader(req.Body).ReadToEnd();
            if (data == string.Empty)
            {
                data = "{}";
            }

            var genRoutine = new MainGenerationRoutine();
            genRoutine.Execute(data);

            return new OkObjectResult(string.Empty);
        }
    }
}
