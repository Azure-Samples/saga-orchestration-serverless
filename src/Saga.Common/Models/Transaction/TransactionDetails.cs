namespace Saga.Common.Models.Transaction
{
    public class TransactionDetails
    {
        public string AccountFromId { get; set; }
        public string AccountToId { get; set; }
        public decimal Amount { get; set; }
    }
}
