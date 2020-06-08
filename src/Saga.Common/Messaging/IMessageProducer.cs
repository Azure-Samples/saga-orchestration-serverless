using System.Threading.Tasks;

namespace Saga.Common.Messaging
{
    public interface IMessageProducer
    {
        Task ProduceAsync(object message);
    }
}
