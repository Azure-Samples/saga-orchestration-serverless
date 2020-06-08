using Saga.Common.Messaging;
using Saga.Common.Enums;
using Saga.Common.Events;
using Saga.Common.Models.Error;
using System;

namespace Saga.Participants.Transfer.Factories
{
    public static class TransferServiceEventFactory
    {
        public static TransferSucceededEvent BuildTransferSucceededEvent(string transactionId)
        {
            return new TransferSucceededEvent()
            {
                Header = BuildEventHeader(transactionId, nameof(TransferSucceededEvent))
            };
        }

        public static OtherReasonTransferFailedEvent BuildOtherReasonTransferFailedEvent(string transactionId, Exception exception)
        {
            return new OtherReasonTransferFailedEvent()
            {
                Header = BuildEventHeader(transactionId, nameof(OtherReasonTransferFailedEvent)),
                Content = new OtherReasonTransferFailedEventContent()
                {
                    Error = BuildErrorDetailsFromException(exception)
                }
            };
        }

        public static TransferCanceledEvent BuildTransferCanceledEvent(string transactionId)
        {
            return new TransferCanceledEvent()
            {
                Header = BuildEventHeader(transactionId, nameof(TransferCanceledEvent)),
            };
        }

        private static MessageHeader BuildEventHeader(string transactionId, string messageType)
        {
            return new MessageHeader(transactionId, messageType, Sources.Transfer.ToString());
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
