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
using Saga.Participants.Validator.Factories;
using Saga.Participants.Validator.Models;

namespace Saga.Functions.Services.Participants
{
    public static class ValidatorService
    {
        [FunctionName(nameof(Validator))]
        public static async Task Validator(
          [EventHubTrigger(@"%ValidatorEventHubName%", Connection = @"EventHubsNamespaceConnection")] EventData[] eventsData,
          [EventHub(@"%ReplyEventHubName%", Connection = @"EventHubsNamespaceConnection")]IAsyncCollector<EventData> eventCollector,
          [CosmosDB(
        databaseName: @"%CosmosDbDatabaseName%",
        collectionName: @"%CosmosDbValidatorCollectionName%",
        ConnectionStringSetting = @"CosmosDbConnectionString")]
            IAsyncCollector<InitialTransfer> stateCollector,
            ILogger logger)
        {
            IMessageProducer eventProducer = new EventHubMessageProducer(eventCollector);
            IRepository<InitialTransfer> repository = new CosmosDbRepository<InitialTransfer>(stateCollector);
            var processors = ValidatorServiceCommandProcessorFactory.BuildProcessorMap(eventProducer, repository);
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
