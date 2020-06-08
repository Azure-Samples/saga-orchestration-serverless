# Configuring the Azure Functions Application Settings

The solution requires a set of application settings on the Azure Functions environment. 

## Configurations

### Cosmos DB configuration

| Property                | Description  |
|-------------------------|--------|
| CosmosDbConnectionString | The Cosmos DB connection string. It must be in the following format: `AccountEndpoint=https://<database name>.documents.azure.com:443/;AccountKey=<key>` |
| CosmosDbDatabaseName | The Cosmos DB database name |
| CosmosDbOrchestratorCollectionName | The collection name on Cosmos DB for the `Saga Orchestrator` service|
| CosmosDbValidatorCollectionName | The collection name on Cosmos DB for the `Validator` service |
| CosmosDbTransferCollectionName | The collection name on Cosmos DB for the `Transfer` service |
| CosmosDbReceiptCollectionName | The collection name on Cosmos DB for the `Receipt` service |
| CosmosDbSagaCollectionName | The collection name on Cosmos DB for Saga observability |

### Event Hubs Configuration

| Property                | Description  |
|-------------------------|--------|
| EventHubsNamespaceConnection | The Event Hubs namespace connection string. It must be in the following format: `Endpoint=sb://<namespace name>.servicebus.windows.net/;SharedAccessKeyName=<key name>;SharedAccessKey=<key>` |
| ValidatorEventHubName | The Event Hub name for the `Validator` service |
| TransferEventHubName | The Event Hub name for the `Transfer` service |
| ReceiptEventHubName | The Event Hub name for the `Receipt` service |
| ReplyEventHubName | The Event Hub name for the `Saga Event Processor` service |

### Retry configuration

| Property                | Description  |
|-------------------------|--------|
| EventHubsProducerMaxRetryAttempts | Max number of retry attempts for producing commands and events to Event Hubs |
| ActivityMaxRetryAttempts | Max number of retry attempts for calling the `Command Producer Activity` and `Saga Orchestrator Activity` functions |
| ActivityRetryInterval | Retry interval in seconds for calling the `Command Producer Activity` and `Saga Orchestrator Activity` functions |

### Circuit Breaker configuration

| Property                | Description  |
|-------------------------|--------|
| EventHubsProducerExceptionsAllowedBeforeBreaking | Max number of exceptions allowed on producing commands and events to Event Hubs before breaking the circuit |
| EventHubsProducerBreakDuration | Duration in seconds of the break on the Event Hubs producer |

### Timeout configuration

| Property                | Description  |
|-------------------------|--------|
| ValidatorTimeoutSeconds | Timeout in seconds for waiting a state response from the `Validator` service in the orchestrator  |
| TransferTimeoutSeconds | Timeout in seconds for waiting a state response from the `Transfer` service in the orchestrator  |
| ReceiptTimeoutSeconds | Timeout in seconds for waiting a state response from the `Receipt` service in the orchestrator  |

### Receipt service configuration

In order to simplify the simulation of compensating transaction scenarios, two additional properties were added:

| Property                | Description  |
|-------------------------|--------|
| CreateRandomReceiptResult | Flag for creating random state results (successful or failed operations). If the flag is set to `false`, the `Receipt` service will always return successful operations.  |
| ReceiptSuccessProbability | Success probability percentage on random state results for the `Receipt` service. When set to `100`, means that 100% of the requests will generate successful operations, while `0` mean that all requests will generate failed operations. |

## Updating settings

For running the solution locally, make sure you have the `local.settings.json` file under the `Saga.Orchestration` and `Saga.Participants` projects.

```json
{
  "IsEncrypted": false,
  "Values": {
    "CosmosDbConnectionString": "AccountEndpoint=https://<database name>.documents.azure.com:443/;AccountKey=<key>",
    "CosmosDbDatabaseName": "sagadb",
    "CosmosDbOrchestratorCollectionName": "orchestrator",
    "CosmosDbValidatorCollectionName": "validator",
    "CosmosDbTransferCollectionName": "transfer",
    "CosmosDbReceiptCollectionName": "receipt",
    "CosmosDbSagaCollectionName": "saga",
    
    "EventHubsNamespaceConnection": "Endpoint=sb://<namespace name>.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=<key>",
    "ValidatorEventHubName": "validator",
    "TransferEventHubName": "transfer",
    "ReceiptEventHubName": "receipt",
    "ReplyEventHubName": "saga-reply",

    "EventHubsProducerMaxRetryAttempts": 3,
    "ActivityMaxRetryAttempts": 3,
    "ActivityRetryInterval": 5,

    "EventHubsProducerExceptionsAllowedBeforeBreaking": 5,
    "EventHubsProducerBreakDuration": 10,

    "ValidatorTimeoutSeconds": 30,
    "TransferTimeoutSeconds": 30,
    "ReceiptTimeoutSeconds": 30,

    "CreateRandomReceiptResult": true,
    "ReceiptSuccessProbability": 0,

    "AzureWebJobsStorage": "<storage connection string>",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet"
  }
}
```
> Note: `AzureWebJobsStorage` can be set to use an Azure storage account or to use an Azure Storage Emulator that acts as a single storage account. For setting up the emulator, follow the [Use the Azure storage emulator for development and testing](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator) instructions.
