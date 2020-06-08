using Saga.Common.Messaging;
using Saga.Common.Commands;
using Saga.Common.Processors;
using Saga.Common.Repository;
using Saga.Participants.Transfer.Factories;
using Saga.Participants.Transfer.Models;
using System;
using System.Threading.Tasks;

namespace Saga.Participants.Transfer.Processors
{
    public class CancelTransferCommandProcessor : ICommandProcessor
    {
        private readonly IMessageProducer eventProducer;
        private readonly IRepository<CheckingAccountLine> repository;

        public CancelTransferCommandProcessor(IMessageProducer eventProducer, IRepository<CheckingAccountLine> repository)
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
                await CancelTransferAsync(header, content);
            }
            catch (Exception exception)
            {
                await ProcessCancellationFailureAsync(header, exception);
            }
        }

        private async Task CancelTransferAsync(MessageHeader header, CancelTransferCommandContent content)
        {
            var transactionId = header?.TransactionId;
            var transaction = content?.Transaction;

            var transfer = new CheckingAccountTransfer();
            Event tranferCancelled = transfer.CancelTranferAmountBetweenAccounts(transactionId, transaction.AccountFromId, transaction.AccountToId, transaction.Amount);

            foreach (var line in transfer.CheckingAccountLines)
                await repository.AddAsync(line);

            await eventProducer.ProduceAsync(tranferCancelled);
        }

        private async Task ProcessCancellationFailureAsync(MessageHeader header, Exception exception)
        {
            var transferFailed = TransferServiceEventFactory.BuildOtherReasonTransferFailedEvent(header.TransactionId, exception);
            await eventProducer.ProduceAsync(transferFailed);
        }
    }
}
