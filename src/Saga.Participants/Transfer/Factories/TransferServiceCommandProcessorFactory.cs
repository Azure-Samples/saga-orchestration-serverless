using Saga.Common.Messaging;
using Saga.Common.Commands;
using Saga.Common.Processors;
using Saga.Common.Repository;
using Saga.Participants.Transfer.Processors;
using Saga.Participants.Transfer.Models;
using System.Collections.Generic;

namespace Saga.Participants.Transfer.Factories
{
    public class TransferServiceCommandProcessorFactory
    {
        public static IDictionary<string, ICommandProcessor> BuildProcessorMap(IMessageProducer eventProducer, IRepository<CheckingAccountLine> repository)
        {
            return new Dictionary<string, ICommandProcessor>
            {
                { nameof(TransferCommand), new TransferCommandProcessor(eventProducer, repository) },
                { nameof(CancelTransferCommand), new CancelTransferCommandProcessor(eventProducer, repository) }
            };
        }
    }
}
