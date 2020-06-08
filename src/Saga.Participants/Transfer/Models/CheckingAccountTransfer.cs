using System;
using Saga.Common.Messaging;
using Saga.Participants.Transfer.Factories;
using System.Collections.Generic;

namespace Saga.Participants.Transfer.Models
{
    public class CheckingAccountTransfer
    {
        private readonly List<CheckingAccountLine> operations;

        public IEnumerable<CheckingAccountLine> CheckingAccountLines => operations.AsReadOnly();

        public CheckingAccountTransfer()
        {
            operations = new List<CheckingAccountLine>();
        }

        public Event TranferAmountBetweenAccounts(string transactionId, string fromAccountId, string toAccountId, decimal amount)
        {
            Validate(transactionId, fromAccountId, toAccountId, amount);

            var debitFrom = new CheckingAccountLine(transactionId, fromAccountId, -amount, GetTransferingDescription(toAccountId));
            operations.Add(debitFrom);

            var creditTo = new CheckingAccountLine(transactionId, toAccountId, +amount, GetTransferingDescription(fromAccountId));
            operations.Add(creditTo);

            return TransferServiceEventFactory.BuildTransferSucceededEvent(transactionId);
        }

        private string GetTransferingDescription(string accountId)
        {
            return string.Format(ConstantStrings.TransferingMessage, accountId);
        }

        public Event CancelTranferAmountBetweenAccounts(string transactionId, string fromAccountId, string toAccountId, decimal amount)
        {
            Validate(transactionId, fromAccountId, toAccountId, amount);

            var creditFrom = new CheckingAccountLine(transactionId, fromAccountId, +amount, GetRevertingMessage());
            operations.Add(creditFrom);

            var debitTo = new CheckingAccountLine(transactionId, toAccountId, -amount, GetRevertingMessage());
            operations.Add(debitTo);

            return TransferServiceEventFactory.BuildTransferCanceledEvent(transactionId);
        }

        private string GetRevertingMessage()
        {
            return ConstantStrings.RevertingMessage;
        }

        private void Validate(string transactionId, string fromAccountId, string toAccountId, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(transactionId)) throw new NullReferenceException(nameof(transactionId));
            if (string.IsNullOrWhiteSpace(fromAccountId)) throw new NullReferenceException(nameof(fromAccountId));
            if (string.IsNullOrWhiteSpace(toAccountId)) throw new NullReferenceException(nameof(toAccountId));
            if (amount <= 0) throw new ArgumentException("Amount needs to be greater than 0", nameof(amount));
        }
    }
}
