using Newtonsoft.Json;
using Saga.Common.Messaging;
using Saga.Participants.Validator.Factories;

namespace Saga.Participants.Validator.Models
{
    public class InitialTransfer
    {
        [JsonProperty("id")]
        public string TransactionId { get; }

        [JsonProperty("from")]
        public Account From { get; private set; }

        [JsonProperty("to")]
        public Account To { get; private set; }

        [JsonProperty("amount")]
        public Amount Amount { get; private set; }

        [JsonProperty("state")]
        public InitialTransferState State { get; private set; }

        public InitialTransfer(string id, string fromId, string toId, decimal amount)
        {
            TransactionId = id;
            From = new Account(fromId);
            To = new Account(toId);
            Amount = new Amount(amount);
            State = InitialTransferState.NONE;
        }

        public Event ValidateTransfer()
        {
            if (string.IsNullOrWhiteSpace(From.Id))
            {
                State = InitialTransferState.INVALID;
                return ValidatorServiceEventFactory.BuildInvalidAccountEvent(TransactionId, ConstantStrings.ProcessAccountToError);
            }

            if (string.IsNullOrWhiteSpace(To.Id))
            {
                State = InitialTransferState.INVALID;
                return ValidatorServiceEventFactory.BuildInvalidAccountEvent(TransactionId, ConstantStrings.ProcessAccountFromError);
            }

            if (Amount.Value <= 0)
            {
                State = InitialTransferState.INVALID;
                return ValidatorServiceEventFactory.BuildInvalidAmountEvent(TransactionId, ConstantStrings.ProcessAmountError);
            }

            State = InitialTransferState.VALID;
            return ValidatorServiceEventFactory.BuildTransferValidatedEvent(TransactionId);
        }

        public Event CancelTransfer()
        {
            State = InitialTransferState.CANCELLED;
            return ValidatorServiceEventFactory.BuildTransferCanceledEvent(TransactionId);
        }
    }
}
