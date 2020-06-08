using System;
using Microsoft.Azure.Documents.Client;

namespace Saga.Orchestration.Utils
{
    public static class UriUtils
    {
        public static Uri CreateTransactionCollectionUri()
        {
            string databaseName = Environment.GetEnvironmentVariable("CosmosDbDatabaseName");
            string collectionName = Environment.GetEnvironmentVariable("CosmosDbOrchestratorCollectionName");

            return UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);
        }
    }
}
