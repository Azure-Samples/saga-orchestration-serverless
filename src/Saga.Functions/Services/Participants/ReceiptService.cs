using System;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Saga.Common.Messaging;
using Saga.Common.Processors;
using Saga.Functions.Factories;
using Saga.Common.Repository;
using Saga.Functions.Messaging;
using Saga.Functions.Repository;
using Saga.Participants.Receipt.Factories;
using Saga.Participants.Receipt.Models;

namespace Saga.Functions.Services.Participants
{
    public static class ReceiptService
    {
        [FunctionName(nameof(ReceiptCreator))]
        public static async Task ReceiptCreator(
          [EventHubTrigger(@"%ReceiptEventHubName%", Connection = @"EventHubsNamespaceConnection")] EventData[] eventsData,
          [EventHub(@"%ReplyEventHubName%", Connection = @"EventHubsNamespaceConnection")]IAsyncCollector<EventData> eventCollector,
          [CosmosDB(
        databaseName: @"%CosmosDbDatabaseName%",
        collectionName: @"%CosmosDbReceiptCollectionName%",
        ConnectionStringSetting = @"CosmosDbConnectionString")]
      IAsyncCollector<ExecutedTransfer> stateCollector,
          ILogger logger)
        {
            IMessageProducer eventProducer = new EventHubMessageProducer(eventCollector);
            IRepository<ExecutedTransfer> repository = new CosmosDbRepository<ExecutedTransfer>(stateCollector);
            var processors = ReceiptServiceCommandProcessorFactory.BuildProcessorMap(eventProducer, repository);
            var dispatcher = new CommandProcessorDispatcher(processors);

            foreach (EventData eventData in eventsData)
            {
                try
                {
                    var commandContainer = CommandContainerFactory.BuildCommandContainer(eventData);
                    await dispatcher.ProcessCommandAsync(commandContainer);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }
            }
        }
    }
}
