using Saga.Common.Messaging;
using Saga.Common.Models.Transaction;

namespace Saga.Common.Commands
{
    public class IssueReceiptCommand : Command
    {
        public IssueReceiptCommandContent Content { get; set; }
    }

    public class IssueReceiptCommandContent
    {
        public TransactionDetails Transaction { get; set; }
    }
}
