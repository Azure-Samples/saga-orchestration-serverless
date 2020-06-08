using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Saga.Common.Commands;
using Saga.Orchestration.Models.Producer;

namespace Saga.Functions.Services.Activities
{
    public static class ProducerActivity
    {
        [FunctionName(nameof(ValidateTransferCommandProducerActivity))]
        public static async Task<ProducerResult> ValidateTransferCommandProducerActivity(
          [EventHub(@"%ValidatorEventHubName%", Connection = @"EventHubsNamespaceConnection")]IAsyncCollector<EventData> messagesCollector,
          [ActivityTrigger] ValidateTransferCommand command,
          ILogger log)
        {
            Producer producer = new Producer(messagesCollector, log);
            return await producer.ProduceCommandWithRetryAsync(command);
        }

        [FunctionName(nameof(TransferCommandProducerActivity))]
        public static async Task<ProducerResult> TransferCommandProducerActivity(
          [EventHub(@"%TransferEventHubName%", Connection = @"EventHubsNamespaceConnection")]IAsyncCollector<EventData> messagesCollector,
          [ActivityTrigger] TransferCommand command,
          ILogger log)
        {
            Producer producer = new Producer(messagesCollector, log);
            return await producer.ProduceCommandWithRetryAsync(command);
        }

        [FunctionName(nameof(CancelTransferCommandProducerActivity))]
        public static async Task<ProducerResult> CancelTransferCommandProducerActivity(
          [EventHub(@"%TransferEventHubName%", Connection = @"EventHubsNamespaceConnection")]IAsyncCollector<EventData> messagesCollector,
          [ActivityTrigger] CancelTransferCommand command,
          ILogger log)
        {
            Producer producer = new Producer(messagesCollector, log);
            return await producer.ProduceCommandWithRetryAsync(command);
        }

        [FunctionName(nameof(ReceiptCommandProducerActivity))]
        public static async Task<ProducerResult> ReceiptCommandProducerActivity(
          [EventHub(@"%ReceiptEventHubName%", Connection = @"EventHubsNamespaceConnection")]IAsyncCollector<EventData> messagesCollector,
          [ActivityTrigger] IssueReceiptCommand command,
          ILogger log)
        {
            Producer producer = new Producer(messagesCollector, log);
            return await producer.ProduceCommandWithRetryAsync(command);
        }
    }
}
