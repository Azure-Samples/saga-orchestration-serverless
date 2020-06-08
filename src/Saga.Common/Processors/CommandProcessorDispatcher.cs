using Saga.Common.Messaging;
using Saga.Common.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Saga.Common.Processors
{
    public class CommandProcessorDispatcher
    {
        private readonly IDictionary<string, ICommandProcessor> processors;

        public CommandProcessorDispatcher(IDictionary<string, ICommandProcessor> processors)
        {
            if (processors == null || !processors.Any())
            {
                throw new ArgumentException();
            }

            this.processors = processors;
        }

        public async Task ProcessCommandAsync(ICommandContainer commandContainer)
        {
            if (commandContainer == null)
            {
                throw new ArgumentException();
            }

            MessageHeader header = GetCommandHeader(commandContainer);
            string commandType = GetCommandType(header);

            await DispatchCommandAsync(commandContainer, commandType);
        }

        private MessageHeader GetCommandHeader(ICommandContainer commandContainer)
        {
            var defaultCommand = commandContainer.ParseCommand<DefaultCommand>();
            return defaultCommand.Header;
        }

        private string GetCommandType(MessageHeader header)
        {
            return header.MessageType;
        }

        private async Task DispatchCommandAsync(ICommandContainer commandContainer, string commandType)
        {
            if (processors.ContainsKey(commandType))
                await processors[commandType].ProcessAsync(commandContainer);
        }
    }
}
