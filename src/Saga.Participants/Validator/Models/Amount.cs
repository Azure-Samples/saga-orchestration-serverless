using Newtonsoft.Json;

namespace Saga.Participants.Validator.Models
{
    public class Amount
    {
        [JsonProperty("value")]
        public decimal Value { get; set; }

        public Amount(decimal value)
        {
            Value = value;
        }
    }
}
