using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Saga.Functions.Models;
using Saga.Orchestration.Models.Transaction;

namespace Saga.Functions.Services
{
    public class SagaStarterService
    {
        [FunctionName(nameof(SagaStarter))]
        public static async Task<IActionResult> SagaStarter(
          [HttpTrigger(AuthorizationLevel.Function, methods: "post", Route = "saga/start")] TransactionItem item,
          HttpRequest request,
          [DurableClient] IDurableOrchestrationClient client,
          ILogger log)
        {
            RequestInputResult result = ValidateInput(item);

            if (!result.Valid)
            {
                return new BadRequestObjectResult($@"The {result.PropertyName} value is invalid");
            }

            string instanceId = await client.StartNewAsync(nameof(Orchestrator.SagaOrchestrator), item.Id, item);

            log.LogInformation(string.Format(ConstantStrings.TransactionStarted, instanceId));

            var response = new SagaStarterResponse
            {
                TransactionId = instanceId
            };

            return new OkObjectResult(response);
        }

        private static RequestInputResult ValidateInput(TransactionItem item)
        {
            if (item == null)
            {
                return new RequestInputResult
                {
                    Valid = false,
                    PropertyName = nameof(item)
                };
            }

            if (string.IsNullOrEmpty(item.AccountFromId))
            {
                return new RequestInputResult
                {
                    Valid = false,
                    PropertyName = nameof(item.AccountFromId)
                };
            }

            if (string.IsNullOrEmpty(item.AccountToId))
            {
                return new RequestInputResult
                {
                    Valid = false,
                    PropertyName = nameof(item.AccountToId)
                };
            }

            if (item.Amount <= 0)
            {
                return new RequestInputResult
                {
                    Valid = false,
                    PropertyName = nameof(item.Amount)
                };
            }

            return new RequestInputResult();
        }
    }
}
