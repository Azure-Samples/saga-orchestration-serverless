using Newtonsoft.Json;
using Saga.Common.Messaging;
using Saga.Common.Utils;
using Saga.Participants.Receipt.Factories;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Saga.Participants.Receipt.Models
{
    public class ExecutedTransfer
    {
        [JsonProperty("id")]
        public string TransactionId { get; }

        [JsonProperty("transferDate")]
        public DateTime TransferDate { get; }

        [JsonProperty("state")]
        public ExecutedTransferState State { get; private set; }

        [JsonProperty("receiptSignature")]
        public string ReceiptSignature { get; private set; }

        public ExecutedTransfer(string transactionId)
        {
            TransactionId = transactionId;
            TransferDate = SystemTime.Now;
            State = ExecutedTransferState.NONE;
            ReceiptSignature = string.Empty;
        }

        public Event IssueReceipt()
        {
            State = ExecutedTransferState.ISSUED;
            ReceiptSignature = Hash();

            return ReceiptServiceEventFactory.BuildReceiptIssuedEvent(TransactionId, ReceiptSignature);
        }

        private string Hash()
        {
            using HashAlgorithm hash = SHA256.Create();

            string serialized = this.ToString();
            byte[] commandBytes = Encoding.ASCII.GetBytes(serialized);

            byte[] hashBytes = hash.ComputeHash(commandBytes);

            StringBuilder sb = new StringBuilder();
            foreach (var b in hashBytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public override string ToString()
        {
            return $"id={TransactionId},date={TransferDate},state={State}";
        }
    }
}
