using System;
using System.Collections.Generic;
using Saga.Orchestration.Models;
using Saga.Orchestration.Models.Transaction;

namespace Saga.Functions.Tests.Services.Activities
{
    public class OrchestratorActivityTestsBase : TestBase
    {
        private static readonly string transactionId = Guid.NewGuid().ToString();
        private static readonly string accountFromId = Guid.NewGuid().ToString();
        private static readonly string accountToId = Guid.NewGuid().ToString();

        public static IEnumerable<object[]> OrchestratorActivityInputData => new List<object[]>
        {
            new object[] {
                new TransactionItem
                {
                    Id = transactionId,
                    AccountFromId = accountFromId,
                    AccountToId = accountToId,
                    Amount = 100.00M,
                    State = nameof(SagaState.Pending)
                },
                new TransactionItem
                {
                    Id = transactionId,
                    AccountFromId = accountFromId,
                    AccountToId = accountToId,
                    Amount = 100.00M,
                    State = nameof(SagaState.Success)
                }
            },
            new object[] {
                new TransactionItem
                {
                    Id = transactionId,
                    AccountFromId = accountFromId,
                    AccountToId = accountToId,
                    Amount = 100.00M,
                    State = nameof(SagaState.Pending)
                },
                new TransactionItem
                {
                    Id = transactionId,
                    AccountFromId = accountFromId,
                    AccountToId = accountToId,
                    Amount = 100.00M,
                    State = nameof(SagaState.Cancelled)
                }
            },
            new object[] {
                new TransactionItem
                {
                    Id = transactionId,
                    AccountFromId = accountFromId,
                    AccountToId = accountToId,
                    Amount = 100.00M,
                    State = nameof(SagaState.Pending)
                },
                new TransactionItem
                {
                    Id = transactionId,
                    AccountFromId = accountFromId,
                    AccountToId = accountToId,
                    Amount = 100.00M,
                    State = nameof(SagaState.Fail)
                }
            }
        };
    }
}
