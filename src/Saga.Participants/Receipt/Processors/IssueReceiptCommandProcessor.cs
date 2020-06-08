using Saga.Common.Messaging;
using Saga.Common.Commands;
using Saga.Common.Processors;
using Saga.Common.Repository;
using Saga.Participants.Receipt.Factories;
using Saga.Participants.Receipt.Models;
using System;
using System.Threading.Tasks;

namespace Saga.Participants.Receipt.Processors
{
    public class IssueReceiptCommandProcessor : ICommandProcessor
    {
        private readonly IMessageProducer eventProducer;
        private readonly IRepository<ExecutedTransfer> repository;

        public IssueReceiptCommandProcessor(IMessageProducer eventProducer, IRepository<ExecutedTransfer> repository)
        {
            this.eventProducer = eventProducer;
            this.repository = repository;
        }

        public async Task ProcessAsync(ICommandContainer commandContainer)
        {
            var validateCommand = commandContainer.ParseCommand<IssueReceiptCommand>();

            var header = validateCommand.Header;
            var content = validateCommand.Content;

            try
            {
                await CreateReceiptAsync(header, content);
            }
            catch (Exception exception)
            {
                await ProcessFailure(header, exception);
            }
        }

        private async Task CreateReceiptAsync(MessageHeader header, IssueReceiptCommandContent content)
        {
            var transactionId = header?.TransactionId;

            var executedTransfer = new ExecutedTransfer(transactionId);
            Event receiptIssued = executedTransfer.IssueReceipt();

            await repository.AddAsync(executedTransfer);
            await eventProducer.ProduceAsync(receiptIssued);
        }

        private async Task ProcessFailure(MessageHeader header, Exception exception)
        {
            var transferFailed = ReceiptServiceEventFactory.BuildOtherReasonReceiptFailedEvent(header.TransactionId, exception);
            await eventProducer.ProduceAsync(transferFailed);
        }
    }
}
