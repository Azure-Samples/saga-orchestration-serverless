using Saga.Common.Processors;

namespace Saga.Participants.Tests.Utils.Stubs
{
    public class CommandContainerStub : ICommandContainer
    {
        private readonly object command;

        public CommandContainerStub(object command)
        {
            this.command = command;
        }

        public T ParseCommand<T>()
        {
            return (T)command;
        }
    }
}
