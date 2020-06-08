using System;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using Saga.Common.Messaging;
using Saga.Common.Commands;
using Saga.Common.Enums;
using Saga.Common.Processors;
using Xunit;
using Saga.Orchestration.Models.Producer;

namespace Saga.Orchestration.Tests
{
    public class ProducerTests : TestBase
    {
        [Fact]
        public async Task Command_should_be_produced_by_valid_ingestion_service()
        {
            var messagesCollectorMock = new Mock<IAsyncCollector<EventData>>();
            var loggerMock = new Mock<ILogger>();

            messagesCollectorMock
              .Setup(x => x.AddAsync(It.IsAny<EventData>(), default))
              .Returns(
                Task.CompletedTask
              );

            var command = new DefaultCommand
            {
                Header = new MessageHeader(Guid.NewGuid().ToString(), nameof(DefaultCommand), nameof(Sources.Orchestrator)),
            };

            Producer producer = new Producer(messagesCollectorMock.Object, loggerMock.Object);
            ProducerResult result = await producer.ProduceCommandWithRetryAsync(command);

            ICommandContainer commandContainer = DeserializeEventData(result.Message);
            MessageHeader header = GetCommandHeader(commandContainer);

            Assert.True(result.Valid);
            Assert.NotEqual(header.MessageId, command.Header.MessageId);
            Assert.NotEqual(header.CreationDate, command.Header.CreationDate);
            Assert.Equal(header.TransactionId, command.Header.TransactionId);
            Assert.Equal(header.MessageType, command.Header.MessageType);
            Assert.Equal(header.Source, command.Header.Source);
        }

        [Fact]
        public async Task Command_should_not_be_produced_by_invalid_ingestion_service()
        {
            IAsyncCollector<EventData> messagesCollector = null;
            var loggerMock = new Mock<ILogger>();

            var command = new DefaultCommand
            {
                Header = new MessageHeader(Guid.NewGuid().ToString(), nameof(DefaultCommand), nameof(Sources.Orchestrator)),
            };

            Producer producer = new Producer(messagesCollector, loggerMock.Object);
            ProducerResult result = await producer.ProduceCommandWithRetryAsync(command);

            Assert.NotNull(result);
            Assert.False(result.Valid);
        }

        [Fact]
        public async Task Command_should_not_be_produced_by_unavailable_ingestion_service()
        {
            var messagesCollectorMock = new Mock<IAsyncCollector<EventData>>();
            var loggerMock = new Mock<ILogger>();

            var command = new DefaultCommand
            {
                Header = new MessageHeader(Guid.NewGuid().ToString(), nameof(DefaultCommand), nameof(Sources.Orchestrator)),
            };

            messagesCollectorMock
              .Setup(x => x.AddAsync(It.IsAny<EventData>(), default))
              .Returns(
                Task.FromException(new Exception())
              );

            Producer producer = new Producer(messagesCollectorMock.Object, loggerMock.Object);
            ProducerResult result = await producer.ProduceCommandWithRetryAsync(command);

            Assert.NotNull(result);
            Assert.False(result.Valid);
        }
    }
}
