using Saga.Common.Messaging;
using Saga.Common.Models.Error;

namespace Saga.Common.Events
{
    public class InvalidAmountEvent : Event
    {
        public InvalidAmountEventContent Content { get; set; }
    }

    public class InvalidAmountEventContent
    {
        public ErrorDetails Error { get; set; }
    }
}
