using Saga.Common.Events;
using Saga.Common.Messaging;
using Saga.Participants.Validator.Models;
using System;
using Xunit;

namespace Saga.Participants.Tests.Validator
{
    public class ValidationTests
    {
        [Theory]
        [InlineData("", "", 100, InitialTransferState.INVALID)]
        [InlineData("", "B", 100, InitialTransferState.INVALID)]
        [InlineData("A", "", 100, InitialTransferState.INVALID)]
        [InlineData("A", "B", -1, InitialTransferState.INVALID)]
        [InlineData("A", "B", 0, InitialTransferState.INVALID)]
        [InlineData("A", "B", 50, InitialTransferState.VALID)]
        public void InitialTransfer_WhenProcessingValidTransaction_CollectsExpectedEvents(string from, string to, decimal amount, InitialTransferState expectedState)
        {
            var transactionId = Guid.NewGuid().ToString();
            var initialTransfer = new InitialTransfer(transactionId, from, to, amount);

            initialTransfer.ValidateTransfer();

            Assert.Equal(expectedState, initialTransfer.State);
        }

        [Theory]
        [InlineData("", "", 100, nameof(InvalidAccountEvent))]
        [InlineData("", "B", 100, nameof(InvalidAccountEvent))]
        [InlineData("A", "", 100, nameof(InvalidAccountEvent))]
        [InlineData("A", "B", -1, nameof(InvalidAmountEvent))]
        [InlineData("A", "B", 0, nameof(InvalidAmountEvent))]
        [InlineData("A", "B", 50, nameof(TransferValidatedEvent))]
        public void InitialTransfer_WhenProcessingValidTransaction_ProducesExpectedEvents(string from, string to, decimal amount, string expectedEvent)
        {
            var transactionId = Guid.NewGuid().ToString();
            var initialTransfer = new InitialTransfer(transactionId, from, to, amount);

            Event @event = initialTransfer.ValidateTransfer();

            Assert.Equal(expectedEvent, @event.Header.MessageType);
        }
    }
}
