using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Saga.Common.Commands;
using Saga.Common.Enums;
using Saga.Common.Events;
using Saga.Orchestration.Factories;
using Saga.Orchestration.Models;
using Saga.Orchestration.Models.Activity;
using Saga.Orchestration.Models.Producer;
using Saga.Orchestration.Models.Transaction;
using Xunit;

namespace Saga.Orchestration.Tests
{
    public class DurableOrchestratorTests : TestBase
    {
        public static IEnumerable<object[]> OrchestratorInputData => new List<object[]>
        {
            new object[] { new Dictionary<string, Func<Task<ActivityResult<ProducerResult>>>>(), null },
            new object[] { null, new Dictionary<string, Func<Task<bool>>>() },
            new object[] { null, null },
        };

        [Theory]
        [MemberData(nameof(OrchestratorInputData))]
        public void Saga_should_not_start_with_invalid_input(
            Dictionary<string, Func<Task<ActivityResult<ProducerResult>>>> commandProducers,
            Dictionary<string, Func<Task<bool>>> sagaStatePersisters)
        {
            var exception = Assert.Throws<ArgumentException>(() => new DurableOrchestrator(commandProducers, sagaStatePersisters));
            Assert.Equal("Invalid orchestrator parameters", exception.Message);
        }

        [Fact]
        public async Task Saga_should_succeed_with_successful_workflow_and_valid_states()
        {
            var transactionItem = CreateFakeTransactionItem();

            var commandProducers = new Dictionary<string, Func<Task<ActivityResult<ProducerResult>>>>
            {
                [nameof(ValidateTransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                }),
                [nameof(TransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                }),
                [nameof(IssueReceiptCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                })
            };

            var sagaStatePersisters = new Dictionary<string, Func<Task<bool>>>
            {
                [nameof(SagaState.Pending)] = () => Task.Run(() => true),
                [nameof(SagaState.Success)] = () => Task.Run(() => true),
            };

            var loggerMock = new Mock<ILogger>();
            var mockContext = new Mock<IDurableOrchestrationContext>();

            mockContext
                .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Validator)))
                .ReturnsAsync(nameof(TransferValidatedEvent));

            mockContext
                .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Transfer)))
                .ReturnsAsync(nameof(TransferSucceededEvent));

            mockContext
                .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Receipt)))
                .ReturnsAsync(nameof(ReceiptIssuedEvent));

            var orchestrator = new DurableOrchestrator(commandProducers, sagaStatePersisters);
            SagaState sagaState = await orchestrator.OrchestrateAsync(transactionItem, mockContext.Object, loggerMock.Object);

            Assert.Equal(SagaState.Success, sagaState);
        }

        [Fact]
        public async Task Saga_should_succeed_with_successful_compensation_workflow()
        {
            var transactionItem = CreateFakeTransactionItem();

            var commandProducers = new Dictionary<string, Func<Task<ActivityResult<ProducerResult>>>>
            {
                [nameof(ValidateTransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                }),
                [nameof(TransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                }),
                [nameof(CancelTransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                }),
                [nameof(IssueReceiptCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                })
            };

            var sagaStatePersisters = new Dictionary<string, Func<Task<bool>>>
            {
                [nameof(SagaState.Pending)] = () => Task.Run(() => true),
                [nameof(SagaState.Success)] = () => Task.Run(() => true),
                [nameof(SagaState.Cancelled)] = () => Task.Run(() => true),
            };

            var loggerMock = new Mock<ILogger>();
            var mockContext = new Mock<IDurableOrchestrationContext>();

            mockContext
                .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Validator)))
                .ReturnsAsync(nameof(TransferValidatedEvent));

            mockContext
                .SetupSequence(x => x.WaitForExternalEvent<string>(nameof(Sources.Transfer)))
                .ReturnsAsync(nameof(TransferSucceededEvent))
                .ReturnsAsync(nameof(TransferCanceledEvent));

            mockContext
                .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Receipt)))
                .ReturnsAsync(nameof(OtherReasonReceiptFailedEvent));

            var orchestrator = new DurableOrchestrator(commandProducers, sagaStatePersisters);
            SagaState sagaState = await orchestrator.OrchestrateAsync(transactionItem, mockContext.Object, loggerMock.Object);

            Assert.Equal(SagaState.Cancelled, sagaState);
        }

        [Fact]
        public async Task Saga_should_fail_with_failed_compensation_workflow()
        {
            var transactionItem = CreateFakeTransactionItem();

            var commandProducers = new Dictionary<string, Func<Task<ActivityResult<ProducerResult>>>>
            {
                [nameof(ValidateTransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                }),
                [nameof(TransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                }),
                [nameof(CancelTransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                }),
                [nameof(IssueReceiptCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                })
            };

            var sagaStatePersisters = new Dictionary<string, Func<Task<bool>>>
            {
                [nameof(SagaState.Pending)] = () => Task.Run(() => true),
                [nameof(SagaState.Success)] = () => Task.Run(() => true),
                [nameof(SagaState.Fail)] = () => Task.Run(() => true),
            };

            var loggerMock = new Mock<ILogger>();
            var mockContext = new Mock<IDurableOrchestrationContext>();

            mockContext
                .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Validator)))
                .ReturnsAsync(nameof(TransferValidatedEvent));

            mockContext
                .SetupSequence(x => x.WaitForExternalEvent<string>(nameof(Sources.Transfer)))
                .ReturnsAsync(nameof(TransferSucceededEvent))
                .ReturnsAsync(nameof(TransferNotCanceledEvent));

            mockContext
                .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Receipt)))
                .ReturnsAsync(nameof(OtherReasonReceiptFailedEvent));

            var orchestrator = new DurableOrchestrator(commandProducers, sagaStatePersisters);
            SagaState sagaState = await orchestrator.OrchestrateAsync(transactionItem, mockContext.Object, loggerMock.Object);

            Assert.Equal(SagaState.Fail, sagaState);
        }


        [Fact]
        public async Task Saga_should_fail_with_invalid_workflow()
        {
            var transactionItem = CreateFakeTransactionItem();

            var commandProducers = new Dictionary<string, Func<Task<ActivityResult<ProducerResult>>>>
            {
                [nameof(ValidateTransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                }),
                [nameof(IssueReceiptCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                })
            };

            var sagaStatePersisters = new Dictionary<string, Func<Task<bool>>>
            {
                [nameof(SagaState.Pending)] = () => Task.Run(() => true),
                [nameof(SagaState.Success)] = () => Task.Run(() => true),
            };

            var loggerMock = new Mock<ILogger>();
            var mockContext = new Mock<IDurableOrchestrationContext>();

            mockContext
                .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Validator)))
                .ReturnsAsync(nameof(TransferValidatedEvent));

            mockContext
                .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Receipt)))
                .ReturnsAsync(nameof(ReceiptIssuedEvent));

            var orchestrator = new DurableOrchestrator(commandProducers, sagaStatePersisters);
            await Assert.ThrowsAsync<KeyNotFoundException>(() => orchestrator.OrchestrateAsync(transactionItem, mockContext.Object, loggerMock.Object));
        }

        [Fact]
        public async Task Saga_should_fail_with_invalid_states()
        {
            var transactionItem = CreateFakeTransactionItem();

            var commandProducers = new Dictionary<string, Func<Task<ActivityResult<ProducerResult>>>>
            {
                [nameof(ValidateTransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                }),
                [nameof(TransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                }),
                [nameof(IssueReceiptCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                })
            };

            var sagaStatePersisters = new Dictionary<string, Func<Task<bool>>>
            {
                [nameof(SagaState.Fail)] = () => Task.Run(() => true)
            };

            var loggerMock = new Mock<ILogger>();
            var mockContext = new Mock<IDurableOrchestrationContext>();

            mockContext
                .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Validator)))
                .ReturnsAsync(nameof(TransferValidatedEvent));

            mockContext
                .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Transfer)))
                .ReturnsAsync(nameof(TransferSucceededEvent));

            mockContext
                .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Receipt)))
                .ReturnsAsync(nameof(ReceiptIssuedEvent));

            var orchestrator = new DurableOrchestrator(commandProducers, sagaStatePersisters);
            await Assert.ThrowsAsync<KeyNotFoundException>(() => orchestrator.OrchestrateAsync(transactionItem, mockContext.Object, loggerMock.Object));
        }

        [Fact]
        public async Task Saga_should_fail_with_invalid_external_events()
        {
            var transactionItem = CreateFakeTransactionItem();

            var commandProducers = new Dictionary<string, Func<Task<ActivityResult<ProducerResult>>>>
            {
                [nameof(ValidateTransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                }),
                [nameof(TransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                }),
                [nameof(IssueReceiptCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                })
            };

            var sagaStatePersisters = new Dictionary<string, Func<Task<bool>>>
            {
                [nameof(SagaState.Fail)] = () => Task.Run(() => true)
            };

            var loggerMock = new Mock<ILogger>();
            var mockContext = new Mock<IDurableOrchestrationContext>();

            var orchestrator = new DurableOrchestrator(commandProducers, sagaStatePersisters);
            await Assert.ThrowsAsync<KeyNotFoundException>(() => orchestrator.OrchestrateAsync(transactionItem, mockContext.Object, loggerMock.Object));
        }

        [Fact]
        public async Task Saga_should_not_be_orchestrated_with_invalid_events()
        {
            var transactionItem = CreateFakeTransactionItem();

            var commandProducers = new Dictionary<string, Func<Task<ActivityResult<ProducerResult>>>>
            {
                [nameof(ValidateTransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                }),
                [nameof(TransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                }),
                [nameof(IssueReceiptCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
                {
                    Item = new ProducerResult()
                })
            };

            var sagaStatePersisters = new Dictionary<string, Func<Task<bool>>>
            {
                [nameof(SagaState.Pending)] = () => Task.Run(() => true),
                [nameof(SagaState.Success)] = () => Task.Run(() => true),
                [nameof(SagaState.Fail)] = () => Task.Run(() => true),
            };

            var loggerMock = new Mock<ILogger>();
            var mockContext = new Mock<IDurableOrchestrationContext>();

            mockContext
                .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Validator)))
                .ReturnsAsync(nameof(ReceiptIssuedEvent));

            var orchestrator = new DurableOrchestrator(commandProducers, sagaStatePersisters);
            SagaState sagaState = await orchestrator.OrchestrateAsync(transactionItem, mockContext.Object, loggerMock.Object);

            Assert.Equal(SagaState.Fail, sagaState);
        }

        private static TransactionItem CreateFakeTransactionItem()
        {
            return new TransactionItem
            {
                Id = Guid.NewGuid().ToString(),
                AccountFromId = Guid.NewGuid().ToString(),
                AccountToId = Guid.NewGuid().ToString(),
                Amount = 100.00M,
                State = SagaState.Pending.ToString()
            };
        }
    }
}
