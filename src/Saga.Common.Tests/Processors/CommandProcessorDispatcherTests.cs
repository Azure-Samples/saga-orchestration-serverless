using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Saga.Common.Commands;
using Saga.Common.Enums;
using Saga.Common.Events;
using Saga.Common.Messaging;
using Saga.Common.Processors;
using Saga.Common.Repository;
using Xunit;

namespace Saga.Common.Tests.Processors
{
    public class CommandProcessorDispatcherTests
    {
        [Fact]
        public async Task Command_should_be_processed_by_valid_dispatcher()
        {
            var messageProducerMock = new Mock<IMessageProducer>();
            var repository = new Mock<IRepository<object>>();
            var commandProcessorMock = new Mock<ICommandProcessor>();
            var commandContainerMock = new Mock<ICommandContainer>();

            messageProducerMock
                .Setup(x => x.ProduceAsync(It.IsAny<DefaultEvent>()))
                .Returns(Task.CompletedTask);

            repository
                .Setup(x => x.AddAsync(It.IsAny<object>()))
                .Returns(Task.CompletedTask);

            commandProcessorMock
                .Setup(x => x.ProcessAsync(It.IsAny<ICommandContainer>()))
                .Returns(Task.CompletedTask);

            commandContainerMock
                .Setup(x => x.ParseCommand<DefaultCommand>())
                .Returns(new DefaultCommand
                {
                    Header = new MessageHeader(Guid.NewGuid().ToString(), nameof(DefaultCommand), nameof(Sources.Orchestrator)),
                });

            var processors = new Dictionary<string, ICommandProcessor>
            {
                { nameof(DefaultCommand), commandProcessorMock.Object }
            };

            var dispatcher = new CommandProcessorDispatcher(processors);
            await dispatcher.ProcessCommandAsync(commandContainerMock.Object);

            Assert.NotNull(dispatcher);
        }

        [Fact]
        public void Command_should_not_be_processed_by_invalid_dispatcher()
        {
            var exception = Assert.Throws<ArgumentException>(() => new CommandProcessorDispatcher(null));
            Assert.NotNull(exception);
        }

        [Fact]
        public void Command_should_not_be_processed_by_invalid_container()
        {
            var commandProcessorMock = new Mock<ICommandProcessor>();

            commandProcessorMock
                .Setup(x => x.ProcessAsync(It.IsAny<ICommandContainer>()))
                .Returns(Task.CompletedTask);

            var processors = new Dictionary<string, ICommandProcessor>
            {
                { nameof(DefaultCommand), commandProcessorMock.Object }
            };

            var dispatcher = new CommandProcessorDispatcher(processors);
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await dispatcher.ProcessCommandAsync(null));

            Assert.NotNull(exception);
        }
    }
}
