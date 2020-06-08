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
using Saga.Participants.Validator.Models;
using Xunit;

namespace Saga.Functions.Tests.Services.Participants
{
    public class ValidatorServiceTests
    {
        [Fact]
        public async Task Validator_processing_should_be_valid()
        {
            EventData[] eventsData = CreateValidatorEventsData();

            var eventCollectorMock = new Mock<IAsyncCollector<EventData>>();
            var stateCollectorMock = new Mock<IAsyncCollector<InitialTransfer>>();
            var loggerMock = new Mock<ILogger>();

            eventCollectorMock
                .Setup(x => x.AddAsync(It.IsAny<EventData>(), default))
                .Returns(Task.CompletedTask);

            stateCollectorMock
                .Setup(x => x.AddAsync(It.IsAny<InitialTransfer>(), default))
                .Returns(Task.CompletedTask);

            await ValidatorService
                .Validator(eventsData, eventCollectorMock.Object, stateCollectorMock.Object, loggerMock.Object);
        }

        [Fact]
        public void Validator_processing_should_be_invalid()
        {
            EventData[] eventsData = CreateValidatorEventsData();

            var stateCollectorMock = new Mock<IAsyncCollector<InitialTransfer>>();
            var loggerMock = new Mock<ILogger>();

            stateCollectorMock
                .Setup(x => x.AddAsync(It.IsAny<InitialTransfer>(), default))
                .Returns(Task.CompletedTask);

            var exception = Assert.ThrowsAsync<Exception>(async () =>
            {
                await ValidatorService
                    .Validator(eventsData, null, null, loggerMock.Object);
            });

            Assert.NotNull(exception);
        }

        private EventData[] CreateValidatorEventsData()
        {
            var command = new ValidateTransferCommand
            {
                Header = new MessageHeader(Guid.NewGuid().ToString(), nameof(ValidateTransferCommand), nameof(Sources.Validator)),
                Content = new ValidateTransferCommandContent
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
