using Saga.Common.Messaging;
using Saga.Common.Enums;
using Saga.Common.Events;
using Saga.Common.Models.Error;
using System;

namespace Saga.Participants.Validator.Factories
{
    public class ValidatorServiceEventFactory
    {
        public static TransferValidatedEvent BuildTransferValidatedEvent(string transactionId)
        {
            return new TransferValidatedEvent()
            {
                Header = BuildEventHeader(transactionId, nameof(TransferValidatedEvent)),
            };
        }

        public static InvalidTransactionEvent BuildInvalidTransactionEvent(string transactionId, string message)
        {
            return new InvalidTransactionEvent()
            {
                Header = BuildEventHeader(transactionId, nameof(InvalidTransactionEvent)),
                Content = new InvalidTransactionEventContent()
                {
                    Error = BuildErrorDetailsFromMessage(message)
                }
            };
        }

        public static InvalidAccountEvent BuildInvalidAccountEvent(string transactionId, string message)
        {
            return new InvalidAccountEvent()
            {
                Header = BuildEventHeader(transactionId, nameof(InvalidAccountEvent)),
                Content = new InvalidAccountEventContent()
                {
                    Error = BuildErrorDetailsFromMessage(message)
                }
            };
        }

        public static InvalidAmountEvent BuildInvalidAmountEvent(string transactionId, string message)
        {
            return new InvalidAmountEvent()
            {
                Header = BuildEventHeader(transactionId, nameof(InvalidAmountEvent)),
                Content = new InvalidAmountEventContent()
                {
                    Error = BuildErrorDetailsFromMessage(message)
                }
            };
        }

        public static OtherReasonValidationFailedEvent BuildOtherReasonValidationFailedEvent(string transactionId, Exception exception)
        {
            return new OtherReasonValidationFailedEvent()
            {
                Header = BuildEventHeader(transactionId, nameof(OtherReasonValidationFailedEvent)),
                Content = new OtherReasonValidationFailedEventContent()
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

        public static TransferNotCanceledEvent BuildTransferNotCanceledEvent(string transactionId, Exception exception)
        {
            return new TransferNotCanceledEvent()
            {
                Header = BuildEventHeader(transactionId, nameof(TransferNotCanceledEvent)),
                Content = new TransferNotCanceledEventContent()
                {
                    Error = BuildErrorDetailsFromException(exception)
                }
            };
        }

        private static MessageHeader BuildEventHeader(string transactionId, string messageType)
        {
            return new MessageHeader(transactionId, messageType, Sources.Validator.ToString());
        }

        private static ErrorDetails BuildErrorDetailsFromMessage(string message)
        {
            return new ErrorDetails()
            {
                Message = message
            };
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
