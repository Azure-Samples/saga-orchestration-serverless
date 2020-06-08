using Saga.Common.Messaging;
using Saga.Common.Models.Error;

namespace Saga.Common.Events
{
    public class OtherReasonValidationFailedEvent : Event
    {
        public OtherReasonValidationFailedEventContent Content { get; set; }
    }

    public class OtherReasonValidationFailedEventContent
    {
        public ErrorDetails Error { get; set; }
    }
}
