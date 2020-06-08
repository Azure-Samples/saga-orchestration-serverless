using Saga.Common.Messaging;
using Saga.Common.Models.Transaction;

namespace Saga.Common.Commands
{
    public class CancelTransferCommand : Command
    {
        public CancelTransferCommandContent Content { get; set; }
    }

    public class CancelTransferCommandContent
    {
        public TransactionDetails Transaction { get; set; }
    }
}
