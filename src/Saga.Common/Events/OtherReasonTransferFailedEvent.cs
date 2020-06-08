using Saga.Common.Messaging;
using Saga.Common.Models.Error;

namespace Saga.Common.Events
{
    public class OtherReasonTransferFailedEvent : Event
    {
        public OtherReasonTransferFailedEventContent Content { get; set; }
    }

    public class OtherReasonTransferFailedEventContent
    {
        public ErrorDetails Error { get; set; }
    }
}
