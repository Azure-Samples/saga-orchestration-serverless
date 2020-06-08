using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Saga.Common.Commands;
using Saga.Functions.Factories;
using Saga.Orchestration.Factories;
using Saga.Orchestration.Models;
using Saga.Orchestration.Models.Activity;
using Saga.Orchestration.Models.Producer;
using Saga.Orchestration.Models.Transaction;

namespace Saga.Functions.Services
{
    public static class Orchestrator
    {
        [FunctionName(nameof(SagaOrchestrator))]
        public static async Task SagaOrchestrator(
          [OrchestrationTrigger] IDurableOrchestrationContext context,
          ILogger log)
        {
            TransactionItem item = context.GetInput<TransactionItem>();

            var commandProducers = new Dictionary<string, Func<Task<ActivityResult<ProducerResult>>>>
            {
                [nameof(ValidateTransferCommand)] = () => ActivityFactory.ProduceValidateTransferCommandAsync(item, context, log),
                [nameof(TransferCommand)] = () => ActivityFactory.ProduceTransferCommandAsync(item, context, log),
                [nameof(CancelTransferCommand)] = () => ActivityFactory.ProduceCancelTransferCommandAsync(item, context, log),
                [nameof(IssueReceiptCommand)] = () => ActivityFactory.ProduceIssueReceiptCommandAsync(item, context, log)
            };

            var sagaStatePersisters = new Dictionary<string, Func<Task<bool>>>
            {
                [nameof(SagaState.Pending)] = () => SagaFactory.PersistSagaStateAsync(item, SagaState.Pending, context, log),
                [nameof(SagaState.Success)] = () => SagaFactory.PersistSagaStateAsync(item, SagaState.Success, context, log),
                [nameof(SagaState.Cancelled)] = () => SagaFactory.PersistSagaStateAsync(item, SagaState.Cancelled, context, log),
                [nameof(SagaState.Fail)] = () => SagaFactory.PersistSagaStateAsync(item, SagaState.Fail, context, log),
            };

            try
            {
                var orchestrator = new DurableOrchestrator(commandProducers, sagaStatePersisters);
                SagaState sagaState = await orchestrator.OrchestrateAsync(item, context, log);

                log.LogInformation($@"Saga state = {nameof(sagaState)} [{context.InstanceId}]");
            }
            catch (ArgumentException ex)
            {
                log.LogError(ex.Message);
            }
        }
    }
}
