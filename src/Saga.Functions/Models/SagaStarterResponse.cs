using Newtonsoft.Json;

namespace Saga.Functions.Models
{
    public class SagaStarterResponse
    {
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }
    }
}
