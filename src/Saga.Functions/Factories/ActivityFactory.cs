using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Saga.Common.Commands;
using Saga.Functions.Services.Activities;
using Saga.Orchestration.Factories;
using Saga.Orchestration.Models.Activity;
using Saga.Orchestration.Models.Producer;
using Saga.Orchestration.Models.Transaction;

namespace Saga.Functions.Factories
{
    public static class ActivityFactory
    {
        private static readonly RetryOptions RetryOptions = new RetryOptions(
          firstRetryInterval: TimeSpan.FromSeconds(int.Parse(Environment.GetEnvironmentVariable("ActivityRetryInterval"))),
          maxNumberOfAttempts: int.Parse(Environment.GetEnvironmentVariable("ActivityMaxRetryAttempts")));

        public static async Task<ActivityResult<T>> CallActivityWithRetryAndTimeoutAsync<T>(Activity<T> activity)
        {
            if (activity == null)
            {
                return new ActivityResult<T>
                {
                    Valid = false,
                    Item = Activator.CreateInstance<T>(),
                    ExceptionMessage = ConstantStrings.ExceptionMessage
                };
            }

            using (var cts = new CancellationTokenSource())
            {
                DateTime deadline = activity.Context.CurrentUtcDateTime.Add(activity.Timeout);
                Task activityTask = activity.Context.CallActivityWithRetryAsync<T>(activity.FunctionName, RetryOptions, activity.Input);
                Task timeoutTask = activity.Context.CreateTimer(deadline, cts.Token);
                Task winner = await Task.WhenAny(activityTask, timeoutTask);

                if (winner != activityTask)
                {
                    return new ActivityResult<T>
                    {
                        Valid = false,
                        Item = Activator.CreateInstance<T>(),
                        ExceptionMessage = string.Format(ConstantStrings.TimeoutError, activity.FunctionName)
                    };
                }

                cts.Cancel();

                return new ActivityResult<T>
                {
                    Item = activity.Input
                };
            }
        }

        public static async Task<ActivityResult<ProducerResult>> ProduceValidateTransferCommandAsync(
          TransactionItem item, IDurableOrchestrationContext context, ILogger log)
        {
            ValidateTransferCommand command = CommandFactory.BuildValidateTransferCommand(item);
            string functionName = nameof(ProducerActivity.ValidateTransferCommandProducerActivity);
            return await RunProducerActivityAsync(functionName, command, context, log);
        }

        public static async Task<ActivityResult<ProducerResult>> ProduceTransferCommandAsync(
          TransactionItem item, IDurableOrchestrationContext context, ILogger log)
        {
            TransferCommand command = CommandFactory.BuildTransferCommand(item);
            string functionName = nameof(ProducerActivity.TransferCommandProducerActivity);
            return await RunProducerActivityAsync(functionName, command, context, log);
        }

        public static async Task<ActivityResult<ProducerResult>> ProduceCancelTransferCommandAsync(
          TransactionItem item, IDurableOrchestrationContext context, ILogger log)
        {
            CancelTransferCommand command = CommandFactory.BuildCancelTransferCommand(item);
            string functionName = nameof(ProducerActivity.CancelTransferCommandProducerActivity);
            return await RunProducerActivityAsync(functionName, command, context, log);
        }

        public static async Task<ActivityResult<ProducerResult>> ProduceIssueReceiptCommandAsync(
          TransactionItem item, IDurableOrchestrationContext context, ILogger log)
        {
            IssueReceiptCommand command = CommandFactory.BuildIssueReceiptCommand(item);
            string functionName = nameof(ProducerActivity.ReceiptCommandProducerActivity);
            return await RunProducerActivityAsync(functionName, command, context, log);
        }

        private static async Task<ActivityResult<ProducerResult>> RunProducerActivityAsync<T>(
          string functionName,
          T item,
          IDurableOrchestrationContext context,
          ILogger log)
        {
            try
            {
                ProducerResult result = await context
                  .CallActivityWithRetryAsync<ProducerResult>(functionName, RetryOptions, item);

                log.LogTrace(string.Format(ConstantStrings.EventCreated, context.InstanceId, functionName, result.Message.Body.Count));

                return new ActivityResult<ProducerResult>
                {
                    Item = result
                };
            }
            catch (FunctionFailedException ex)
            {
                log.LogError(string.Format(ConstantStrings.FunctionFailed, functionName, ex.Message));

                return new ActivityResult<ProducerResult>
                {
                    Valid = false,
                    Item = Activator.CreateInstance<ProducerResult>(),
                    ExceptionMessage = ex.Message
                };
            }
        }
    }
}
