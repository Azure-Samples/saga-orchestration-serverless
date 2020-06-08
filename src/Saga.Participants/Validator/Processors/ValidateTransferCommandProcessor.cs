using System;
using System.Threading.Tasks;
using Saga.Common.Messaging;
using Saga.Common.Commands;
using Saga.Common.Processors;
using Saga.Common.Repository;
using Saga.Participants.Validator.Factories;
using Saga.Participants.Validator.Models;

namespace Saga.Participants.Validator.Processors
{
    public class ValidateTransferCommandProcessor : ICommandProcessor
    {
        private readonly IMessageProducer eventProducer;
        private readonly IRepository<InitialTransfer> repository;

        public ValidateTransferCommandProcessor(IMessageProducer eventProducer, IRepository<InitialTransfer> repository)
        {
            this.eventProducer = eventProducer;
            this.repository = repository;
        }

        public async Task ProcessAsync(ICommandContainer commandContainer)
        {
            var validateCommand = commandContainer.ParseCommand<ValidateTransferCommand>();

            var header = validateCommand.Header;
            var content = validateCommand.Content;

            try
            {
                await ValidateAsync(header, content);
            }
            catch (Exception exception)
            {
                await ProcessFailure(header, exception);
            }
        }

        private async Task ValidateAsync(MessageHeader header, ValidateTransferCommandContent content)
        {
            var transactionId = header?.TransactionId;
            var transaction = content?.Transaction;

            var initialTransfer = new InitialTransfer(transactionId, transaction.AccountFromId, transaction.AccountToId, transaction.Amount);
            Event validationEvent = initialTransfer.ValidateTransfer();

            await repository.AddAsync(initialTransfer);
            await eventProducer.ProduceAsync(validationEvent);
        }

        private async Task ProcessFailure(MessageHeader header, Exception exception)
        {
            Event validationFailed = ValidatorServiceEventFactory.BuildOtherReasonValidationFailedEvent(header.TransactionId, exception);
            await eventProducer.ProduceAsync(validationFailed);
        }
    }
}
