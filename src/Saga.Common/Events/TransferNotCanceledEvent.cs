using Saga.Common.Messaging;
using Saga.Common.Models.Error;

namespace Saga.Common.Events
{
    public class TransferNotCanceledEvent : Event
    {
        public TransferNotCanceledEventContent Content { get; set; }
    }

    public class TransferNotCanceledEventContent
    {
        public ErrorDetails Error { get; set; }
    }
}
