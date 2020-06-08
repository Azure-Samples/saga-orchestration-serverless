using Saga.Common.Messaging;
using Saga.Common.Commands;
using Saga.Common.Processors;
using Saga.Common.Repository;
using Saga.Participants.Validator.Factories;
using System;
using System.Threading.Tasks;
using Saga.Participants.Validator.Models;

namespace Saga.Participants.Validator.Processors
{
    public class CancelTransferCommandProcessor : ICommandProcessor
    {
        private readonly IMessageProducer eventProducer;
        private readonly IRepository<InitialTransfer> repository;

        public CancelTransferCommandProcessor(IMessageProducer eventProducer, IRepository<InitialTransfer> repository)
        {
            this.eventProducer = eventProducer;
            this.repository = repository;
        }

        public async Task ProcessAsync(ICommandContainer commandContainer)
        {
            var validateCommand = commandContainer.ParseCommand<CancelTransferCommand>();

            var header = validateCommand.Header;
            var content = validateCommand.Content;

            try
            {
                await CancelTransfer(header, content);
            }
            catch (Exception exception)
            {
                await ProcessCancellationFailure(header, exception);
            }
        }

        private async Task CancelTransfer(MessageHeader header, CancelTransferCommandContent content)
        {
            var transactionId = header?.TransactionId;
            var transaction = content?.Transaction;

            var initialTransfer = new InitialTransfer(transactionId, transaction.AccountFromId, transaction.AccountToId, transaction.Amount);
            Event transferCanceled = initialTransfer.CancelTransfer();

            await repository.AddAsync(initialTransfer);
            await eventProducer.ProduceAsync(transferCanceled);
        }

        private async Task ProcessCancellationFailure(MessageHeader header, Exception exception)
        {
            Event cancelValidationEvent = ValidatorServiceEventFactory.BuildTransferNotCanceledEvent(header.TransactionId, exception);
            await eventProducer.ProduceAsync(cancelValidationEvent);
        }
    }
}
