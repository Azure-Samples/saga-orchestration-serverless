using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Saga.Common.Commands;
using Saga.Common.Enums;
using Saga.Common.Messaging;
using Saga.Common.Models.Transaction;
using Saga.Functions.Services.Participants;
using Saga.Participants.Receipt.Models;
using Xunit;

namespace Saga.Functions.Tests.Services.Participants
{
    public class ReceiptServiceTests
    {
        [Fact]
        public async Task Receipt_processing_should_be_valid()
        {
            EventData[] eventsData = CreateIssueReceiptEventsData();

            var eventCollectorMock = new Mock<IAsyncCollector<EventData>>();
            var stateCollectorMock = new Mock<IAsyncCollector<ExecutedTransfer>>();
            var loggerMock = new Mock<ILogger>();

            eventCollectorMock
                .Setup(x => x.AddAsync(It.IsAny<EventData>(), default))
                .Returns(Task.CompletedTask);

            stateCollectorMock
                .Setup(x => x.AddAsync(It.IsAny<ExecutedTransfer>(), default))
                .Returns(Task.CompletedTask);

            await ReceiptService
                .ReceiptCreator(eventsData, eventCollectorMock.Object, stateCollectorMock.Object, loggerMock.Object);

            eventCollectorMock
                .Verify(x => x.AddAsync(It.IsAny<EventData>(), default), Times.AtLeastOnce());

            stateCollectorMock
                .Verify(x => x.AddAsync(It.IsAny<ExecutedTransfer>(), default), Times.AtLeastOnce());
        }

        [Fact]
        public void Receipt_processing_should_be_invalid()
        {
            EventData[] eventsData = CreateIssueReceiptEventsData();

            var stateCollectorMock = new Mock<IAsyncCollector<ExecutedTransfer>>();
            var loggerMock = new Mock<ILogger>();

            stateCollectorMock
                .Setup(x => x.AddAsync(It.IsAny<ExecutedTransfer>(), default))
                .Returns(Task.CompletedTask);

            var exception = Assert.ThrowsAsync<Exception>(async () =>
            {
                await ValidatorService
                    .Validator(eventsData, null, null, loggerMock.Object);
            });

            Assert.NotNull(exception);
        }

        private EventData[] CreateIssueReceiptEventsData()
        {
            var command = new IssueReceiptCommand
            {
                Header = new MessageHeader(Guid.NewGuid().ToString(), nameof(IssueReceiptCommand), nameof(Sources.Orchestrator)),
                Content = new IssueReceiptCommandContent
                {
                    Transaction = new TransactionDetails
                    {
                        AccountFromId = Guid.NewGuid().ToString(),
                        AccountToId = Guid.NewGuid().ToString(),
                        Amount = 100.00M
                    }
                }
            };

            string serializedMsg = JsonConvert.SerializeObject(command);
            byte[] messageBytes = Encoding.UTF8.GetBytes(serializedMsg);

            return new EventData[]
            {
                new EventData(messageBytes)
            };
        }
    }
}
