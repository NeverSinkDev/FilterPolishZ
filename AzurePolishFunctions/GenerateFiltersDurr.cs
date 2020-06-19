using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FilterEconomy.Facades;
using FilterCore;
using FilterEconomyProcessor;
using System.Linq;
using System.Collections.Generic;
using FilterPolishUtil.Collections;
using FilterEconomy.Model;
using AzurePolishFunctions.DataFileRequests;
using FilterCore.Constants;
using FilterPolishUtil;
using FilterPolishUtil.Model;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using AzurePolishFunctions.Procedures;

namespace AzurePolishFunctions
{
    public static class GenerateFiltersDurr
    {
        public static MainGenerationRoutine FilterGen = new MainGenerationRoutine();

        [FunctionName("GenerateFiltersDurr")]
        public static string Run([ActivityTrigger] string req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                FilterGen.Execute(req, log);
                return "finished generation succesfully!";
            }
            catch (Exception e)
            {
                LoggingFacade.LogError("ERROR: " + e.Message);
                return e.ToString();
            }
        }
    }
}
