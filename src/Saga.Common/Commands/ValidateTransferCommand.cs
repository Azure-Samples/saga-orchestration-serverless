using Saga.Common.Messaging;
using Saga.Common.Models.Transaction;

namespace Saga.Common.Commands
{
    public class ValidateTransferCommand : Command
    {
        public ValidateTransferCommandContent Content { get; set; }
    }

    public class ValidateTransferCommandContent
    {
        public TransactionDetails Transaction { get; set; }
    }
}
