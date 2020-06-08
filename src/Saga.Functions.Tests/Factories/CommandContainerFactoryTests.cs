using System;
using System.Text;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using Saga.Common.Commands;
using Saga.Common.Enums;
using Saga.Common.Messaging;
using Saga.Functions.Factories;
using Xunit;

namespace Saga.Functions.Tests.Factories
{
    public class CommandContainerFactoryTests
    {
        [Fact]
        public void Container_should_be_built_with_valid_event_data()
        {
            var command = new DefaultCommand
            {
                Header = new MessageHeader(Guid.NewGuid().ToString(), nameof(DefaultCommand), nameof(Sources.Orchestrator)),
            };

            string serializedMsg = JsonConvert.SerializeObject(command);
            byte[] messageBytes = Encoding.UTF8.GetBytes(serializedMsg);

            var eventData = new EventData(messageBytes);
            var commandContainer = CommandContainerFactory.BuildCommandContainer(eventData);
            var newCommand = commandContainer.ParseCommand<DefaultCommand>();

            Assert.NotEqual(command.Header.MessageId, newCommand.Header.MessageId);
            Assert.NotEqual(command.Header.CreationDate, newCommand.Header.CreationDate);
            Assert.Equal(command.Header.TransactionId, newCommand.Header.TransactionId);
            Assert.Equal(command.Header.MessageType, newCommand.Header.MessageType);
            Assert.Equal(command.Header.Source, newCommand.Header.Source);
        }
    }
}
