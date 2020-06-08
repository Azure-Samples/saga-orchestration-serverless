using Saga.Common.Events;
using Saga.Common.Messaging;
using Saga.Participants.Transfer.Models;
using System;
using System.Linq;
using Xunit;

namespace Saga.Participants.Tests.Transfer
{
    public class TransferTests
    {
        [Theory]
        [InlineData("idFrom123", "idTo123", 1299.99)]
        [InlineData("idFrom123", "idTo123", 299.99)]
        [InlineData("idFrom123", "idTo123", 0.99)]
        [InlineData("idFrom123", "idTo123", 0.01)]
        public void CheckingAccountTransfer_WhenProcessingValidTransaction_GeneratesDebitOnFromAccount(string from, string to, decimal amount)
        {
            var transactionId = Guid.NewGuid().ToString();
            var transfer = new CheckingAccountTransfer();

            transfer.TranferAmountBetweenAccounts(transactionId, from, to, amount);
            var lines = transfer.CheckingAccountLines;

            Assert.NotNull(lines.SingleOrDefault(l => l.AccountId == from && l.Amount == -amount));
        }

        [Theory]
        [InlineData("idFrom123", "idTo123", 1299.99)]
        [InlineData("idFrom123", "idTo123", 299.99)]
        [InlineData("idFrom123", "idTo123", 0.99)]
        [InlineData("idFrom123", "idTo123", 0.01)]
        public void CheckingAccountTransfer_WhenProcessingValidTransaction_GeneratesCreditOnToAccount(string from, string to, decimal amount)
        {
            var transactionId = Guid.NewGuid().ToString();
            var transfer = new CheckingAccountTransfer();

            transfer.TranferAmountBetweenAccounts(transactionId, from, to, amount);
            var lines = transfer.CheckingAccountLines;

            Assert.NotNull(lines.SingleOrDefault(l => l.AccountId == to && l.Amount == +amount));
        }

        [Theory]
        [InlineData("idFrom123", "idTo123", 1299.99)]
        [InlineData("idFrom123", "idTo123", 299.99)]
        [InlineData("idFrom123", "idTo123", 0.99)]
        [InlineData("idFrom123", "idTo123", 0.01)]
        public void CheckingAccountTransfer_WhenProcessingValidTransaction_ProducesTransferSucceddedEvent(string from, string to, decimal amount)
        {
            var transactionId = Guid.NewGuid().ToString();
            var transfer = new CheckingAccountTransfer();

            Event @event = transfer.TranferAmountBetweenAccounts(transactionId, from, to, amount);

            Assert.Equal(nameof(TransferSucceededEvent), @event.Header.MessageType);
        }

        [Theory]
        [InlineData("idFrom123", "idTo123", -500.00)]
        [InlineData("idFrom123", "idTo123", -299.99)]
        [InlineData("idFrom123", "idTo123", -0.99)]
        [InlineData("idFrom123", "idTo123", -0.01)]
        [InlineData("idFrom123", "idTo123", 0.00)]
        public void CheckingAccountTransfer_WhenProcessingNegativeAmount_ProducesException(string from, string to, decimal amount)
        {
            var transactionId = Guid.NewGuid().ToString();
            var transfer = new CheckingAccountTransfer();

            void action() => transfer.TranferAmountBetweenAccounts(transactionId, from, to, amount);

            Assert.Throws<ArgumentException>(action);
        }
    }
}
