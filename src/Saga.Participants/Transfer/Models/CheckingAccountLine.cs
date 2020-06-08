using Newtonsoft.Json;
using Saga.Common.Utils;
using System;

namespace Saga.Participants.Transfer.Models
{
    public class CheckingAccountLine
    {
        [JsonProperty("id")]
        public Guid TransferId { get; set; }
        [JsonProperty("transactionId")]
        public string TransactionId { get; }
        [JsonProperty("transferDate")]
        public DateTime TransferDate { get; set; }
        [JsonProperty("transferType")]
        public string TransferType { get; set; }
        [JsonProperty("accountId")]
        public string AccountId { get; set; }
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }

        public CheckingAccountLine(string transactionId, string accountId, decimal amount, string description)
        {
            TransferId = Guid.NewGuid();
            TransactionId = transactionId;
            TransferDate = SystemTime.Now;
            TransferType = transactionId;
            AccountId = accountId;
            Amount = amount;
            Description = description;
        }
    }
}
