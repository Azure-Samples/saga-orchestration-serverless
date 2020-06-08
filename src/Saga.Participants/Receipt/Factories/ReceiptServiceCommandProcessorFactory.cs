using Saga.Common.Messaging;
using Saga.Common.Commands;
using Saga.Common.Processors;
using Saga.Common.Repository;
using Saga.Participants.Receipt.Processors;
using Saga.Participants.Receipt.Models;
using System.Collections.Generic;

namespace Saga.Participants.Receipt.Factories
{
    public class ReceiptServiceCommandProcessorFactory
    {
        public static IDictionary<string, ICommandProcessor> BuildProcessorMap(IMessageProducer eventProducer, IRepository<ExecutedTransfer> repository)
        {
            return new Dictionary<string, ICommandProcessor>
            {
                { nameof(IssueReceiptCommand), new IssueReceiptCommandProcessor(eventProducer, repository) },
            };
        }
    }
}
