using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Saga.Functions.Services.Activities;
using Saga.Orchestration.Models;
using Saga.Orchestration.Models.Activity;
using Saga.Orchestration.Models.Transaction;

namespace Saga.Functions.Factories
{
    public static class SagaFactory
    {
        private static readonly string OrchestratorActivityFunction = nameof(OrchestratorActivity.SagaOrchestratorActivity);
        private static readonly int OrchestratorActivityTimeout = int.Parse(Environment.GetEnvironmentVariable("OrchestratorActivityTimeoutSeconds"));

        public static async Task<bool> PersistSagaStateAsync(
          TransactionItem transactionItem, SagaState state, IDurableOrchestrationContext context, ILogger log)
        {
            TransactionItem item = TransactionFactory.BuildTransactionItemByState(transactionItem, state);
            ActivityResult<TransactionItem> result = await PersistSagaStateAsync(context, item);

            if (!result.Valid)
            {
                log.LogError(result.ExceptionMessage);
                return false;
            }

            return true;
        }

        private static async Task<ActivityResult<TransactionItem>> PersistSagaStateAsync(
          IDurableOrchestrationContext context, TransactionItem item)
        {
            var finishedItem = new TransactionItem
            {
                Id = item.Id,
                AccountFromId = item.AccountFromId,
                AccountToId = item.AccountToId,
                Amount = item.Amount,
                State = item.State.ToString()
            };

            var activity = new Activity<TransactionItem>
            {
                FunctionName = OrchestratorActivityFunction,
                Input = finishedItem,
                Context = context,
                Timeout = TimeSpan.FromSeconds(OrchestratorActivityTimeout)
            };

            return await ActivityFactory.CallActivityWithRetryAndTimeoutAsync(activity);
        }
    }
}
