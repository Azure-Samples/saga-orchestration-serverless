using Newtonsoft.Json.Linq;

namespace Saga.Common.Processors
{
    public class CommandContainer : ICommandContainer
    {
        private readonly JObject jObject;

        public CommandContainer(JObject jObject)
        {
            this.jObject = jObject;
        }

        public T ParseCommand<T>()
        {
            return jObject.ToObject<T>();
        }
    }
}
