using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Saga.Common.Events;
using Saga.Orchestration.Models;

namespace SagaOrchestration.Services
{
    public static class EventProcessor
    {
        [FunctionName(nameof(SagaEventProcessor))]
        public static async Task SagaEventProcessor(
          [EventHubTrigger(@"%ReplyEventHubName%", Connection = @"EventHubsNamespaceConnection")] EventData[] eventsData,
          [CosmosDB(
        databaseName: @"%CosmosDbDatabaseName%",
        collectionName: @"%CosmosDbSagaCollectionName%",
        ConnectionStringSetting = @"CosmosDbConnectionString")] IAsyncCollector<SagaItem> documentCollector,
          [DurableClient] IDurableOrchestrationClient client,
          ILogger log)
        {
            foreach (EventData eventData in eventsData)
            {
                var jsonBody = Encoding.UTF8.GetString(eventData.Body);
                DefaultEvent @event = JsonConvert.DeserializeObject<DefaultEvent>(jsonBody);

                if (@event.Header == null)
                {
                    log.LogError("Invalid event header");
                    continue;
                }

                var item = new SagaItem
                {
                    TransactionId = @event.Header.TransactionId,
                    MessageType = @event.Header.MessageType,
                    Source = @event.Header.Source,
                    CreationDate = @event.Header.CreationDate
                };

                await documentCollector.AddAsync(item);
                await client.RaiseEventAsync(@event.Header.TransactionId, @event.Header.Source, @event.Header.MessageType);
            }
        }
    }
}
