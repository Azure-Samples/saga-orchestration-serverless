using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Saga.Functions.Factories;
using Saga.Functions.Services.Activities;
using Saga.Orchestration.Models;
using Saga.Orchestration.Models.Activity;
using Saga.Orchestration.Models.Transaction;
using Xunit;

namespace Saga.Functions.Tests.Factories
{
    public class ActivityFactoryTests : TestBase
    {
        [Fact]
        public async Task Activity_call_with_valid_payload_should_be_valid()
        {
            var mockContext = new Mock<IDurableOrchestrationContext>();

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

            ActivityResult<TransactionItem> result = await ActivityFactory.CallActivityWithRetryAndTimeoutAsync(activity);

            Assert.True(result.Valid);
            Assert.NotNull(result.Item);
            Assert.Equal(result.ExceptionMessage, string.Empty);
        }

        [Fact]
        public async Task Activity_call_with_invalid_payload_should_be_invalid()
        {
            Activity<TransactionItem> activity = null;
            ActivityResult<TransactionItem> result = await ActivityFactory.CallActivityWithRetryAndTimeoutAsync(activity);

            Assert.False(result.Valid);
            Assert.NotNull(result.Item);
            Assert.NotEmpty(result.ExceptionMessage);
        }
    }
}
