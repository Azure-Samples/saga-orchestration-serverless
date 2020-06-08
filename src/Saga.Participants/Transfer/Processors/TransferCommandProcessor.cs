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
    public class TransferCommandProcessor : ICommandProcessor
    {
        private readonly IMessageProducer eventProducer;
        private readonly IRepository<CheckingAccountLine> repository;

        public TransferCommandProcessor(IMessageProducer eventProducer, IRepository<CheckingAccountLine> repository)
        {
            this.eventProducer = eventProducer;
            this.repository = repository;
        }

        public async Task ProcessAsync(ICommandContainer commandContainer)
        {
            var validateCommand = commandContainer.ParseCommand<TransferCommand>();

            var header = validateCommand.Header;
            var content = validateCommand.Content;

            try
            {
                await TransferAsync(header, content);
            }
            catch (Exception exception)
            {
                await ProcessFailure(header, exception);
            }
        }

        private async Task TransferAsync(MessageHeader header, TransferCommandContent content)
        {
            var transactionId = header?.TransactionId;
            var transaction = content?.Transaction;

            var transfer = new CheckingAccountTransfer();
            Event transferSucceded = transfer.TranferAmountBetweenAccounts(transactionId, transaction.AccountFromId, transaction.AccountToId, transaction.Amount);

            foreach (var line in transfer.CheckingAccountLines)
                await repository.AddAsync(line);

            await eventProducer.ProduceAsync(transferSucceded);
        }

        private async Task ProcessFailure(MessageHeader header, Exception exception)
        {
            var transferFailed = TransferServiceEventFactory.BuildOtherReasonTransferFailedEvent(header.TransactionId, exception);
            await eventProducer.ProduceAsync(transferFailed);
        }
    }

}
