using System;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using Saga.Functions.Services.Participants;
using Saga.Participants.Transfer.Models;
using Xunit;

namespace Saga.Functions.Tests.Services.Participants
{
    public class TransferServiceTests : TransferServiceTestsBase
    {
        [Theory]
        [MemberData(nameof(TransferFunctionInputData))]
        public async Task Transfer_processing_should_be_valid(EventData[] eventsData)
        {
            var eventCollectorMock = new Mock<IAsyncCollector<EventData>>();
            var stateCollectorMock = new Mock<IAsyncCollector<CheckingAccountLine>>();
            var loggerMock = new Mock<ILogger>();

            eventCollectorMock
                .Setup(x => x.AddAsync(It.IsAny<EventData>(), default))
                .Returns(Task.CompletedTask);

            stateCollectorMock
                .Setup(x => x.AddAsync(It.IsAny<CheckingAccountLine>(), default))
                .Returns(Task.CompletedTask);

            await TransferService
                .TransferMoney(eventsData, eventCollectorMock.Object, stateCollectorMock.Object, loggerMock.Object);

            eventCollectorMock
                .Verify(x => x.AddAsync(It.IsAny<EventData>(), default), Times.AtLeastOnce());

            stateCollectorMock
                .Verify(x => x.AddAsync(It.IsAny<CheckingAccountLine>(), default), Times.AtLeastOnce());
        }

        [Theory]
        [MemberData(nameof(TransferFunctionInputData))]
        public void Transfer_processing_should_be_invalid(EventData[] eventsData)
        {
            var stateCollectorMock = new Mock<IAsyncCollector<CheckingAccountLine>>();
            var loggerMock = new Mock<ILogger>();

            stateCollectorMock
                .Setup(x => x.AddAsync(It.IsAny<CheckingAccountLine>(), default))
                .Returns(Task.CompletedTask);

            var exception = Assert.ThrowsAsync<Exception>(async () =>
            {
                await TransferService
                    .TransferMoney(eventsData, null, null, loggerMock.Object);
            });

            Assert.NotNull(exception);
        }
    }
}
