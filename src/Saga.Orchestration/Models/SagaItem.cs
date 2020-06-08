using System;
using Newtonsoft.Json;

namespace Saga.Orchestration.Models
{
    public class SagaItem
    {
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("messageType")]
        public string MessageType { get; set; }

        [JsonProperty("creationDate")]
        public DateTime CreationDate { get; set; }
    }
}
