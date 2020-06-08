using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Saga.Common.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Saga.Functions.Messaging
{
    public class EventHubMessageProducer : IMessageProducer
    {
        private readonly IAsyncCollector<EventData> eventCollector;

        public EventHubMessageProducer(IAsyncCollector<EventData> eventCollector)
        {
            this.eventCollector = eventCollector;
        }

        public async Task ProduceAsync(object message)
        {
            EventData eventData = CreateEventData(message);
            await eventCollector.AddAsync(eventData);
        }

        private EventData CreateEventData(object message)
        {
            string serializedMsg = JsonConvert.SerializeObject(message);
            byte[] messageBytes = Encoding.UTF8.GetBytes(serializedMsg);

            return new EventData(messageBytes);
        }
    }
}
