using Saga.Common.Messaging;
using Saga.Common.Commands;
using Saga.Common.Enums;
using Saga.Common.Models.Transaction;
using Saga.Orchestration.Models.Transaction;

namespace Saga.Orchestration.Factories
{
    public static class CommandFactory
    {
        public static ValidateTransferCommand BuildValidateTransferCommand(TransactionItem item)
        {
            return new ValidateTransferCommand
            {
                Header = BuildEventHeaderFromTransactionId(item.Id, nameof(ValidateTransferCommand)),
                Content = new ValidateTransferCommandContent
                {
                    Transaction = new TransactionDetails
                    {
                        AccountFromId = item.AccountFromId,
                        AccountToId = item.AccountToId,
                        Amount = item.Amount
                    }
                }
            };
        }

        public static TransferCommand BuildTransferCommand(TransactionItem item)
        {
            return new TransferCommand
            {
                Header = BuildEventHeaderFromTransactionId(item.Id, nameof(TransferCommand)),
                Content = new TransferCommandContent
                {
                    Transaction = new TransactionDetails
                    {
                        AccountFromId = item.AccountFromId,
                        AccountToId = item.AccountToId,
                        Amount = item.Amount
                    }
                }
            };
        }

        public static CancelTransferCommand BuildCancelTransferCommand(TransactionItem item)
        {
            return new CancelTransferCommand
            {
                Header = BuildEventHeaderFromTransactionId(item.Id, nameof(CancelTransferCommand)),
                Content = new CancelTransferCommandContent
                {
                    Transaction = new TransactionDetails
                    {
                        AccountFromId = item.AccountFromId,
                        AccountToId = item.AccountToId,
                        Amount = item.Amount
                    }
                }
            };
        }

        public static IssueReceiptCommand BuildIssueReceiptCommand(TransactionItem item)
        {
            return new IssueReceiptCommand
            {
                Header = BuildEventHeaderFromTransactionId(item.Id, nameof(IssueReceiptCommand)),
                Content = new IssueReceiptCommandContent
                {
                    Transaction = new TransactionDetails
                    {
                        AccountFromId = item.AccountFromId,
                        AccountToId = item.AccountToId,
                        Amount = item.Amount
                    }
                }
            };
        }

        private static MessageHeader BuildEventHeaderFromTransactionId(string transactionId, string messageType)
        {
            return new MessageHeader(transactionId, messageType, Sources.Orchestrator.ToString());
        }
    }
}
