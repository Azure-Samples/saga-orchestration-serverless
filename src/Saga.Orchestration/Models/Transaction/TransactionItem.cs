using System;
using Newtonsoft.Json;

namespace Saga.Orchestration.Models.Transaction
{
    public class TransactionItem
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("state")]
        public string State { get; set; } = nameof(SagaState.Pending);

        [JsonProperty("accountFromId")]
        public string AccountFromId { get; set; }

        [JsonProperty("accountToId")]
        public string AccountToId { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }
}
