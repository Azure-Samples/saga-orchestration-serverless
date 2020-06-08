using System.Threading.Tasks;

namespace Saga.Common.Processors
{
    public interface ICommandProcessor
    {
        Task ProcessAsync(ICommandContainer commandContainer);
    }
}