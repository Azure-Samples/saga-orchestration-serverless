using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Saga.Orchestration.Models;
using Saga.Orchestration.Models.Transaction;
using Saga.Orchestration.Utils;

namespace Saga.Functions.Services.Activities
{
    public static class OrchestratorActivity
    {
        [FunctionName(nameof(SagaOrchestratorActivity))]
        public static async Task<TransactionItem> SagaOrchestratorActivity(
          [ActivityTrigger] TransactionItem item,
          [CosmosDB(
        databaseName: @"%CosmosDbDatabaseName%",
        collectionName: @"%CosmosDbOrchestratorCollectionName%",
        ConnectionStringSetting = @"CosmosDbConnectionString")]
      IAsyncCollector<TransactionItem> documentCollector,
          [CosmosDB(
        databaseName: @"%CosmosDbDatabaseName%",
        collectionName: @"%CosmosDbOrchestratorCollectionName%",
        ConnectionStringSetting = @"CosmosDbConnectionString")] IDocumentClient client)
        {
            if (item.State == SagaState.Pending.ToString())
            {
                await documentCollector.AddAsync(item);
                return item;
            }

            Uri collectionUri = UriUtils.CreateTransactionCollectionUri();

            var document = client
                .CreateDocumentQuery(collectionUri)
                .Where(t => t.Id == item.Id)
                .AsEnumerable()
                .FirstOrDefault();

            document.SetPropertyValue("state", item.State);
            await client.ReplaceDocumentAsync(document);
            return item;
        }
    }
}
