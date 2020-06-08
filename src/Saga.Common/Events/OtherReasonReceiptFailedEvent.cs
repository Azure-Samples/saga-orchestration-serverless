using Saga.Common.Messaging;
using Saga.Common.Models.Error;

namespace Saga.Common.Events
{
    public class OtherReasonReceiptFailedEvent : Event
    {
        public OtherReasonReceiptFailedEventContent Content { get; set; }
    }

    public class OtherReasonReceiptFailedEventContent
    {
        public ErrorDetails Error { get; set; }
    }
}
