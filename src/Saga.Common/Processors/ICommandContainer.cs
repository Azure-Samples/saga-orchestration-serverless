namespace Saga.Common.Processors
{
    public interface ICommandContainer
    {
        T ParseCommand<T>();
    }
}