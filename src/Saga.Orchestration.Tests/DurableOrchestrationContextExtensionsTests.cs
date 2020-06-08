namespace Saga.Orchestration.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Moq;
    using Saga.Common.Enums;
    using System.Threading;
    using Saga.Orchestration.Utils;
    using Xunit;


    public class DurableOrchestrationContextExtensionsTests : TestBase
    {
        [Theory]
        [InlineData(60, 2)]
        [InlineData(30, 5)]
        [InlineData(06, 5)]
        public async Task WaitForExternalEvent_Should_Return_Null_When_Task_Slower_Than_Timeout(int taskDelay, int timeoutDelay)
        {
            Sources source = Sources.Validator;
            string externalEventReturn = "FakeEvent";
            var fakeTimeout = TimeSpan.FromSeconds(timeoutDelay);

            var ctx = CreateMockedOrquestratorContext(taskDelay, timeoutDelay, externalEventReturn);

            string testEventNameReturn = 
                await DurableOrchestrationContextExtensions
                   .WaitForExternalEventWithTimeout<string>(ctx.Object, source, fakeTimeout);
            
            Assert.Null(testEventNameReturn);
        }

        [Theory]
        [InlineData(0, 5)]
        [InlineData(1, 5)]
        [InlineData(5, 6)]
        public async Task WaitForExternalEvent_Should_Return_Successful_When_Task_Faster_Than_Timeout(int taskDelay, int timeoutDelay)
        {

            Sources source = Sources.Validator;
            string externalEventReturn = "FakeEvent";
            var fakeTimeout = TimeSpan.FromSeconds(timeoutDelay);

            var ctx = CreateMockedOrquestratorContext(taskDelay, timeoutDelay, externalEventReturn);

            string testEventNameReturn =
                await DurableOrchestrationContextExtensions
                   .WaitForExternalEventWithTimeout<string>(ctx.Object, source, fakeTimeout);

            Assert.Equal(externalEventReturn, testEventNameReturn);
        }

        private Mock<IDurableOrchestrationContext> CreateMockedOrquestratorContext(int taskDelay, int timeoutDelay, string externalEventReturn)
        {
            var ctx = new Mock<IDurableOrchestrationContext>();
            var t = Task<string>.Run(async delegate
            {
                await Task.Delay(TimeSpan.FromSeconds(taskDelay));
                return externalEventReturn;
            });

            var timer = Task<string>.Run(async delegate
            {
                await Task.Delay(TimeSpan.FromSeconds(timeoutDelay));
                return default(string);
            });

            ctx.Setup(x => x.CurrentUtcDateTime)
                .Returns(DateTime.UtcNow);
            ctx.Setup(x => x.CreateTimer<string>(It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(timer);
            ctx.Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Validator)))
                .Returns(t);

            return ctx;
        }
    }
}
