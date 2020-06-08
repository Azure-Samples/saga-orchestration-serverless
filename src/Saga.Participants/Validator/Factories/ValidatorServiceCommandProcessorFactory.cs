using System.Collections.Generic;
using Saga.Common.Messaging;
using Saga.Common.Commands;
using Saga.Common.Processors;
using Saga.Common.Repository;
using Saga.Participants.Validator.Processors;
using Saga.Participants.Validator.Models;

namespace Saga.Participants.Validator.Factories
{
    public class ValidatorServiceCommandProcessorFactory
    {
        public static IDictionary<string, ICommandProcessor> BuildProcessorMap(IMessageProducer eventProducer, IRepository<InitialTransfer> repository)
        {
            return new Dictionary<string, ICommandProcessor>
            {
                { nameof(ValidateTransferCommand), new ValidateTransferCommandProcessor(eventProducer, repository) },
                { nameof(CancelTransferCommand), new CancelTransferCommandProcessor(eventProducer, repository) }
            };
        }
    }
}
