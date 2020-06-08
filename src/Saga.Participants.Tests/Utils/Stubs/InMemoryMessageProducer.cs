using Saga.Common.Messaging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saga.Participants.Tests.Utils.Stubs
{
    public class InMemoryMessageProducer : IMessageProducer
    {
        public List<object> Items { get; set; }

        public InMemoryMessageProducer()
        {
            Items = new List<object>();
        }

        public Task ProduceAsync(object message)
        {
            Items.Add(message);
            return Task.CompletedTask;
        }
    }
}
