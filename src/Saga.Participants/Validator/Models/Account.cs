using Newtonsoft.Json;

namespace Saga.Participants.Validator.Models
{
    public class Account
    {
        [JsonProperty("accountId")]
        public string Id { get; set; }

        public Account(string id)
        {
            Id = id;
        }
    }
}
