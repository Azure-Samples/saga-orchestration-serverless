using System.Text;
using Microsoft.Azure.EventHubs;

namespace Saga.Orchestration.Models.Producer
{
    public class ProducerResult
    {
        public bool Valid { get; set; } = true;
        public EventData Message { get; set; } = new EventData(Encoding.UTF8.GetBytes(string.Empty));
    }
}
