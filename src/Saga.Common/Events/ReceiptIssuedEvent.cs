using Saga.Common.Messaging;

namespace Saga.Common.Events
{
    public class ReceiptIssuedEvent : Event
    {
        public ReceiptIssuedEventContent Content { get; set; }
    }

    public class ReceiptIssuedEventContent
    {
        public string ReceiptSignature { get; set; }
    }
}
