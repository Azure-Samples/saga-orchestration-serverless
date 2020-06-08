using System;
using Saga.Orchestration.Models;
using Saga.Orchestration.Models.Transaction;

namespace Saga.Functions.Factories
{
    public static class TransactionFactory
    {
        public static TransactionItem BuildTransactionItemByState(TransactionItem item, SagaState state)
        {
            return new TransactionItem
            {
                Id = item.Id,
                AccountFromId = item.AccountFromId,
                AccountToId = item.AccountToId,
                Amount = item.Amount,
                State = state.ToString()
            };
        }
    }
}
