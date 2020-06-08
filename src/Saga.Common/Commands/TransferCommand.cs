using Saga.Common.Messaging;
using Saga.Common.Models.Transaction;

namespace Saga.Common.Commands
{
    public class TransferCommand : Command
    {
        public TransferCommandContent Content { get; set; }
    }

    public class TransferCommandContent
    {
        public TransactionDetails Transaction { get; set; }
    }
}
