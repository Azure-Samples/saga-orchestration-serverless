using System;
using System.Text;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Newtonsoft.Json;
using Saga.Common.Commands;
using Saga.Common.Enums;
using Saga.Common.Events;
using Saga.Common.Messaging;
using Saga.Orchestration.Models;
using Saga.Orchestration.Models.Producer;
using Saga.Orchestration.Models.Transaction;

namespace Saga.Functions.Tests.Utils
{
    public static class OrchestrationContextMockExtensions
    {
        public static void SetupCommandProducers(this Mock<IDurableOrchestrationContext> contextMock)
        {
            contextMock
                .Setup(x => x.CallActivityWithRetryAsync<ProducerResult>(It.IsAny<string>(), It.IsAny<RetryOptions>(), It.IsAny<object>()))
                .ReturnsAsync(CreateValidProducerResult(new ValidateTransferCommand
                {
                    Header = new MessageHeader(Guid.NewGuid().ToString(), nameof(ValidateTransferCommand), nameof(Sources.Validator)),
                    Content = new ValidateTransferCommandContent()
                }));

            contextMock
                .Setup(x => x.CallActivityWithRetryAsync<ProducerResult>(It.IsAny<string>(), It.IsAny<RetryOptions>(), It.IsAny<object>()))
                .ReturnsAsync(CreateValidProducerResult(new TransferCommand
                {
                    Header = new MessageHeader(Guid.NewGuid().ToString(), nameof(TransferCommand), nameof(Sources.Transfer)),
                    Content = new TransferCommandContent()
                }));

            contextMock
                .Setup(x => x.CallActivityWithRetryAsync<ProducerResult>(It.IsAny<string>(), It.IsAny<RetryOptions>(), It.IsAny<object>()))
                .ReturnsAsync(CreateValidProducerResult(new CancelTransferCommand
                {
                    Header = new MessageHeader(Guid.NewGuid().ToString(), nameof(CancelTransferCommand), nameof(Sources.Transfer)),
                    Content = new CancelTransferCommandContent()
                }));

            contextMock
                .Setup(x => x.CallActivityWithRetryAsync<ProducerResult>(It.IsAny<string>(), It.IsAny<RetryOptions>(), It.IsAny<object>()))
                .ReturnsAsync(CreateValidProducerResult(new IssueReceiptCommand
                {
                    Header = new MessageHeader(Guid.NewGuid().ToString(), nameof(IssueReceiptCommand), nameof(Sources.Receipt)),
                    Content = new IssueReceiptCommandContent()
                }));
        }

        public static void SetupSagaPersisters(this Mock<IDurableOrchestrationContext> contextMock)
        {
            contextMock
                .Setup(x => x.CallActivityWithRetryAsync<TransactionItem>(It.IsAny<string>(), It.IsAny<RetryOptions>(), It.IsAny<object>()))
                .ReturnsAsync(CreateValidTransactionItem(SagaState.Pending));

            contextMock
                .Setup(x => x.CallActivityWithRetryAsync<TransactionItem>(It.IsAny<string>(), It.IsAny<RetryOptions>(), It.IsAny<object>()))
                .ReturnsAsync(CreateValidTransactionItem(SagaState.Success));

            contextMock
                .Setup(x => x.CallActivityWithRetryAsync<TransactionItem>(It.IsAny<string>(), It.IsAny<RetryOptions>(), It.IsAny<object>()))
                .ReturnsAsync(CreateValidTransactionItem(SagaState.Cancelled));

            contextMock
                .Setup(x => x.CallActivityWithRetryAsync<TransactionItem>(It.IsAny<string>(), It.IsAny<RetryOptions>(), It.IsAny<object>()))
                .ReturnsAsync(CreateValidTransactionItem(SagaState.Fail));
        }

        public static void SetupDurableOrchestratorInput(this Mock<IDurableOrchestrationContext> contextMock)
        {
            contextMock
                .Setup(x => x.GetInput<TransactionItem>())
                .Returns(new TransactionItem
                {
                    Id = Guid.NewGuid().ToString(),
                    AccountFromId = Guid.NewGuid().ToString(),
                    AccountToId = Guid.NewGuid().ToString(),
                    Amount = 100.00M,
                    State = nameof(SagaState.Pending)
                });
        }

        public static void SetupDurableOrchestratorExternalEvents(this Mock<IDurableOrchestrationContext> contextMock)
        {
            contextMock
               .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Validator)))
               .ReturnsAsync(nameof(TransferValidatedEvent));

            contextMock
                .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Transfer)))
                .ReturnsAsync(nameof(TransferSucceededEvent));

            contextMock
                .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Receipt)))
                .ReturnsAsync(nameof(ReceiptIssuedEvent));
        }

        private static TransactionItem CreateValidTransactionItem(SagaState state)
        {
            return new TransactionItem
            {
                Id = Guid.NewGuid().ToString(),
                AccountFromId = Guid.NewGuid().ToString(),
                AccountToId = Guid.NewGuid().ToString(),
                Amount = 100.00M,
                State = nameof(state)
            };
        }

        private static ProducerResult CreateValidProducerResult(Command command)
        {
            return new ProducerResult
            {
                Message = CreateEventData(command)
            };
        }

        private static EventData CreateEventData(Command command)
        {
            string serializedMsg = JsonConvert.SerializeObject(command);
            byte[] messageBytes = Encoding.UTF8.GetBytes(serializedMsg);
            return new EventData(messageBytes);
        }
    }
}
