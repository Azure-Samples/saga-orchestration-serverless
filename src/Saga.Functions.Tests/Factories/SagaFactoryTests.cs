using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Saga.Functions.Factories;
using Saga.Functions.Services.Activities;
using Saga.Orchestration.Models;
using Saga.Orchestration.Models.Activity;
using Saga.Orchestration.Models.Transaction;
using Xunit;

namespace Saga.Functions.Tests.Factories
{
    public class SagaFactoryTests : TestBase
    {
        [Fact]
        public async Task Pending_saga_should_be_persisted()
        {
            var mockContext = new Mock<IDurableOrchestrationContext>();
            var loggerMock = new Mock<ILogger>();

            var retryOptions = new RetryOptions(
              firstRetryInterval: TimeSpan.FromSeconds(5),
              maxNumberOfAttempts: 3);

            var item = new TransactionItem
            {
                Id = Guid.NewGuid().ToString(),
                AccountFromId = Guid.NewGuid().ToString(),
                AccountToId = Guid.NewGuid().ToString(),
                Amount = 100.00M,
                State = SagaState.Pending.ToString()
            };

            var activity = new Activity<TransactionItem>
            {
                FunctionName = nameof(OrchestratorActivity.SagaOrchestratorActivity),
                Input = item,
                Context = mockContext.Object
            };

            mockContext
              .Setup(x => x.CallActivityWithRetryAsync<TransactionItem>(activity.FunctionName, retryOptions, activity.Input))
              .ReturnsAsync(item);

            var result = await SagaFactory.PersistSagaStateAsync(item, SagaState.Pending, mockContext.Object, loggerMock.Object);

            Assert.True(result);
        }

        [Fact]
        public async Task Successful_saga_should_be_persisted()
        {
            var mockContext = new Mock<IDurableOrchestrationContext>();
            var loggerMock = new Mock<ILogger>();

            var retryOptions = new RetryOptions(
              firstRetryInterval: TimeSpan.FromSeconds(5),
              maxNumberOfAttempts: 3);

            var item = new TransactionItem
            {
                Id = Guid.NewGuid().ToString(),
                AccountFromId = Guid.NewGuid().ToString(),
                AccountToId = Guid.NewGuid().ToString(),
                Amount = 100.00M,
                State = SagaState.Pending.ToString()
            };

            var activity = new Activity<TransactionItem>
            {
                FunctionName = nameof(OrchestratorActivity.SagaOrchestratorActivity),
                Input = item,
                Context = mockContext.Object
            };

            mockContext
              .Setup(x => x.CallActivityWithRetryAsync<TransactionItem>(activity.FunctionName, retryOptions, activity.Input))
              .ReturnsAsync(item);

            var result = await SagaFactory.PersistSagaStateAsync(item, SagaState.Success, mockContext.Object, loggerMock.Object);

            Assert.True(result);
        }

        [Fact]
        public async Task Failed_saga_should_be_persisted()
        {
            var mockContext = new Mock<IDurableOrchestrationContext>();
            var loggerMock = new Mock<ILogger>();

            var retryOptions = new RetryOptions(
              firstRetryInterval: TimeSpan.FromSeconds(5),
              maxNumberOfAttempts: 3);

            var item = new TransactionItem
            {
                Id = Guid.NewGuid().ToString(),
                AccountFromId = Guid.NewGuid().ToString(),
                AccountToId = Guid.NewGuid().ToString(),
                Amount = 100.00M,
                State = SagaState.Pending.ToString()
            };

            var activity = new Activity<TransactionItem>
            {
                FunctionName = nameof(OrchestratorActivity.SagaOrchestratorActivity),
                Input = item,
                Context = mockContext.Object
            };

            mockContext
              .Setup(x => x.CallActivityWithRetryAsync<TransactionItem>(activity.FunctionName, retryOptions, activity.Input))
              .ReturnsAsync(item);

            var result = await SagaFactory.PersistSagaStateAsync(item, SagaState.Fail, mockContext.Object, loggerMock.Object);

            Assert.True(result);
        }
    }
}
