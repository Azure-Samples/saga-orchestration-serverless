using Saga.Common.Messaging;
using Saga.Common.Models.Error;

namespace Saga.Common.Events
{
    public class InvalidTransactionEvent : Event
    {
        public InvalidTransactionEventContent Content { get; set; }
    }

    public class InvalidTransactionEventContent
    {
        public ErrorDetails Error { get; set; }
    }
}
