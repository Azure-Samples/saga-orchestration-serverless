using Saga.Common.Messaging;
using Saga.Common.Models.Error;

namespace Saga.Common.Events
{
    public class InvalidAccountEvent : Event
    {
        public InvalidAccountEventContent Content { get; set; }
    }

    public class InvalidAccountEventContent
    {
        public ErrorDetails Error { get; set; }
    }
}
