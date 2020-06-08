using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Saga.Common.Events;
using Saga.Orchestration.Models;
using SagaOrchestration.Services;
using Xunit;

namespace Saga.Functions.Tests.Services
{
    public class EventProcessorTests : EventProcessorTestsBase
    {
        [Theory]
        [MemberData(nameof(InputData))]
        public async Task Events_should_be_processed(EventData[] eventsData)
        {
            var clientMock = new Mock<IDurableOrchestrationClient>();
            var documentCollectorMock = new Mock<IAsyncCollector<SagaItem>>();
            var loggerMock = new Mock<ILogger>();

            clientMock
                .Setup(x => x.RaiseEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            documentCollectorMock
                .Setup(x => x.AddAsync(It.IsAny<SagaItem>(), default))
                .Returns(Task.CompletedTask);

            await EventProcessor
                .SagaEventProcessor(eventsData, documentCollectorMock.Object, clientMock.Object, loggerMock.Object);

            int expectedClientExecutionTimes = CountEvents;
            int expectedCollectorExecutionTimes = CountEvents;

            clientMock
                .Verify(x => x.RaiseEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(expectedClientExecutionTimes));

            documentCollectorMock
                .Verify(x => x.AddAsync(It.IsAny<SagaItem>(), default), Times.Exactly(expectedCollectorExecutionTimes));
        }

        [Fact]
        public async Task Invalid_event_should_not_be_processed()
        {
            var clientMock = new Mock<IDurableOrchestrationClient>();
            var documentCollectorMock = new Mock<IAsyncCollector<SagaItem>>();
            var loggerMock = new Mock<ILogger>();

            var defaultEvent = new DefaultEvent
            {
                Header = null
            };

            EventData[] invalidEventsData = CreateInvalidEventsData();

            await EventProcessor
                .SagaEventProcessor(invalidEventsData, documentCollectorMock.Object, clientMock.Object, loggerMock.Object);

            clientMock
                .Verify(x => x.RaiseEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());

            documentCollectorMock
                .Verify(x => x.AddAsync(It.IsAny<SagaItem>(), default), Times.Never());
        }
    }
}
