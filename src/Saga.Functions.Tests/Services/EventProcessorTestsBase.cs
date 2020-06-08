using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using Saga.Common.Enums;
using Saga.Common.Events;
using Saga.Common.Messaging;

namespace Saga.Functions.Tests.Services
{
    public class EventProcessorTestsBase
    {
        public static int CountEvents;

        public static IEnumerable<object[]> InputData => new List<object[]>
        {
            new object[] {
                CreateEvents()
            }
        };

        private static EventData[] CreateEvents()
        {
            var eventsData = new EventData[]
            {
                CreateDefaultEvent(),
                CreateInvalidAccountEvent(),
                CreateInvalidAmountEvent(),
                CreateInvalidTransactionEvent(),
                CreateOtherReasonReceiptFailedEvent(),
                CreateOtherReasonTransferFailedEvent(),
                CreateOtherReasonValidationFailedEvent(),
                CreateReceiptIssuedEvent(),
                CreateTransferCanceledEvent(),
                CreateTransferNotCanceledEvent(),
                CreateTransferSucceededEvent(),
                CreateTransferValidatedEvent()
            };

            CountEvents = eventsData.Count();

            return eventsData;
        }

        private static EventData CreateDefaultEvent()
        {
            var defaultEvent = new DefaultEvent
            {
                Header = CreateDefaultMessageHeader(nameof(DefaultEvent), nameof(Sources.Processor))
            };
            return CreateEventData(defaultEvent);
        }

        private static EventData CreateInvalidAccountEvent()
        {
            var invalidAccountEvent = new InvalidAccountEvent
            {
                Header = CreateDefaultMessageHeader(nameof(InvalidAccountEvent), nameof(Sources.Validator)),
                Content = new InvalidAccountEventContent()
            };

            return CreateEventData(invalidAccountEvent);
        }

        private static EventData CreateInvalidAmountEvent()
        {
            var invalidAmountEvent = new InvalidAmountEvent
            {
                Header = CreateDefaultMessageHeader(nameof(InvalidAmountEvent), nameof(Sources.Validator)),
                Content = new InvalidAmountEventContent()
            };

            return CreateEventData(invalidAmountEvent);
        }

        private static EventData CreateInvalidTransactionEvent()
        {
            var invalidTransactionEvent = new InvalidTransactionEvent
            {
                Header = CreateDefaultMessageHeader(nameof(InvalidTransactionEvent), nameof(Sources.Validator)),
                Content = new InvalidTransactionEventContent()
            };

            return CreateEventData(invalidTransactionEvent);
        }

        private static EventData CreateOtherReasonReceiptFailedEvent()
        {
            var otherReasonReceiptFailed = new OtherReasonReceiptFailedEvent
            {
                Header = CreateDefaultMessageHeader(nameof(OtherReasonReceiptFailedEvent), nameof(Sources.Receipt)),
                Content = new OtherReasonReceiptFailedEventContent()
            };

            return CreateEventData(otherReasonReceiptFailed);
        }

        private static EventData CreateOtherReasonTransferFailedEvent()
        {
            var otherReasonTransferFailedEvent = new OtherReasonTransferFailedEvent
            {
                Header = CreateDefaultMessageHeader(nameof(OtherReasonTransferFailedEvent), nameof(Sources.Transfer)),
                Content = new OtherReasonTransferFailedEventContent()
            };

            return CreateEventData(otherReasonTransferFailedEvent);
        }

        private static EventData CreateOtherReasonValidationFailedEvent()
        {
            var otherReasonValidationFailed = new OtherReasonValidationFailedEvent
            {
                Header = CreateDefaultMessageHeader(nameof(OtherReasonValidationFailedEvent), nameof(Sources.Validator)),
                Content = new OtherReasonValidationFailedEventContent()
            };

            return CreateEventData(otherReasonValidationFailed);
        }

        private static EventData CreateReceiptIssuedEvent()
        {
            var receiptIssuedEvent = new ReceiptIssuedEvent
            {
                Header = CreateDefaultMessageHeader(nameof(ReceiptIssuedEvent), nameof(Sources.Receipt)),
                Content = new ReceiptIssuedEventContent()
            };

            return CreateEventData(receiptIssuedEvent);
        }

        private static EventData CreateTransferCanceledEvent()
        {
            var transferCanceledEvent = new TransferCanceledEvent
            {
                Header = CreateDefaultMessageHeader(nameof(TransferCanceledEvent), nameof(Sources.Transfer))
            };

            return CreateEventData(transferCanceledEvent);
        }

        private static EventData CreateTransferNotCanceledEvent()
        {
            var transferNotCanceledEvent = new TransferNotCanceledEvent
            {
                Header = CreateDefaultMessageHeader(nameof(TransferNotCanceledEvent), nameof(Sources.Transfer)),
                Content = new TransferNotCanceledEventContent()
            };

            return CreateEventData(transferNotCanceledEvent);
        }

        private static EventData CreateTransferSucceededEvent()
        {
            var transferSucceededEvent = new TransferSucceededEvent
            {
                Header = CreateDefaultMessageHeader(nameof(TransferSucceededEvent), nameof(Sources.Transfer))
            };

            return CreateEventData(transferSucceededEvent);
        }

        private static EventData CreateTransferValidatedEvent()
        {
            var transferValidatedEvent = new TransferValidatedEvent
            {
                Header = CreateDefaultMessageHeader(nameof(TransferValidatedEvent), nameof(Sources.Validator))
            };

            return CreateEventData(transferValidatedEvent);
        }

        protected static EventData[] CreateInvalidEventsData()
        {
            var defaultEvent = new DefaultEvent
            {
                Header = null
            };

            return new EventData[]
            {
                CreateEventData(defaultEvent)
            };
        }

        private static EventData CreateEventData(Event sagaEvent)
        {
            string serializedMsg = JsonConvert.SerializeObject(sagaEvent);
            byte[] messageBytes = Encoding.UTF8.GetBytes(serializedMsg);

            return new EventData(messageBytes);
        }

        private static MessageHeader CreateDefaultMessageHeader(string messageType, string source)
        {
            return new MessageHeader(Guid.NewGuid().ToString(), messageType, source);
        }
    }
}
