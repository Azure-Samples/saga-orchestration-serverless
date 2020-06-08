using Saga.Common.Events;
using Saga.Common.Messaging;
using Saga.Participants.Receipt.Models;
using System;
using Xunit;

namespace Saga.Participants.Tests.Receipt
{
    public class ReceiptTests
    {
        [Fact]
        public void ExecutedTransfer_WhenProcessingValidTransaction_IssuesNonEmptyReceipt()
        {
            var transactionId = Guid.NewGuid().ToString();
            var transfer = new ExecutedTransfer(transactionId);

            transfer.IssueReceipt();
            var receipt = transfer.ReceiptSignature;

            Assert.False(string.IsNullOrWhiteSpace(receipt));
        }

        [Fact]
        public void ExecutedTransfer_WhenProcessingValidTransaction_ProducesReceiptIssuedEvent()
        {
            var transactionId = Guid.NewGuid().ToString();
            var transfer = new ExecutedTransfer(transactionId);

            Event @event = transfer.IssueReceipt();

            Assert.NotNull(@event);
            Assert.Equal(nameof(ReceiptIssuedEvent), @event.Header.MessageType);
        }

    }
}
