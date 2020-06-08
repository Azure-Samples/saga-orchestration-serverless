using System;
using Saga.Common.Messaging;
using Saga.Common.Enums;
using Saga.Common.Events;
using Saga.Common.Models.Error;

namespace Saga.Participants.Receipt.Factories
{
    public static class ReceiptServiceEventFactory
    {
        public static ReceiptIssuedEvent BuildReceiptIssuedEvent(string transactionId, string signature)
        {
            return new ReceiptIssuedEvent()
            {
                Header = BuildEventHeader(transactionId, nameof(ReceiptIssuedEvent)),
                Content = new ReceiptIssuedEventContent()
                {
                    ReceiptSignature = signature
                }
            };
        }

        public static OtherReasonReceiptFailedEvent BuildOtherReasonReceiptFailedEvent(string transactionId, Exception exception)
        {
            return new OtherReasonReceiptFailedEvent()
            {
                Header = BuildEventHeader(transactionId, nameof(OtherReasonReceiptFailedEvent)),
                Content = new OtherReasonReceiptFailedEventContent()
                {
                    Error = BuildErrorDetailsFromException(exception)
                }
            };
        }

        private static MessageHeader BuildEventHeader(string transactionId, string messageType)
        {
            return new MessageHeader(transactionId, messageType, Sources.Receipt.ToString());
        }

        private static ErrorDetails BuildErrorDetailsFromException(Exception ex)
        {
            return new ErrorDetails()
            {
                Message = ex.Message
            };
        }
    }
}
