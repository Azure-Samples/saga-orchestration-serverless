# Architecture: Core Components

![Architecture Overview](../img/architecture-overview.jpg)


* **Saga Client:** A Web API implemented as an [Azure Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview?tabs=csharp) with [HTTP trigger binding](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook-trigger?tabs=csharp) that receives HTTP requests to start new transactions. For each request, it generates a random transaction ID, starts a new Saga orchestrator instance and provides the transaction ID as part of the HTTP response.
  
* **Saga Orchestrator:** Long-running [Durable Orchestrator](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-orchestrations?tabs=csharp) that coordinates the transaction workflow by producing *commands* to Event Hubs and waiting for *events* from Saga participants.
  
* **Saga Orchestrator Activity:** [Activity function](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-types-features-overview#activity-functions) with [Cosmos DB binding](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2) that persists the Saga state (*Pending*, *Success* and *Failed*) to Cosmos DB.

* **Command Producer Activity:** [Activity function](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-types-features-overview#activity-functions) with [Event Hubs binding](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-event-hubs) that produces *commands* created by the orchestration to Event Hubs.
  
* **Validator:** Saga participant implemented as an Azure Function with [Event Hubs trigger](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-event-hubs-trigger?tabs=csharp) and [Cosmos DB binding](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2) that simulates a set of bank accounts validation before proceeding to money transfer between accounts (e.g. checking if both accounts exist, if accounts have sufficient balance, etc.). The resulted event (e.g *InvalidAccountEvent*) is produced on `Saga Reply` Event Hubs and persisted on Cosmos DB.

* **Transfer:** Saga participant implemented as an Azure Functions with [Event Hubs trigger](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-event-hubs-trigger?tabs=csharp) and [Cosmos DB binding](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2) that simulates credit and debit operations on bank accounts. The resulted state (e.g. *TransferSucceededEvent*) is produced as on `Saga Reply` Event Hubs and persisted on Cosmos DB.

* **Receipt:** Saga participant implemented as an Azure Function with [Event Hubs trigger](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-event-hubs-trigger?tabs=csharp) and [Cosmos DB binding](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2) that generates a receipt ID for the issuer. The resulted state (e.g. *ReceiptIssuedEvent*) is produced on `Saga Reply` Event Hubs and persisted on Cosmos DB.

* **Saga Event Processor:** [Azure Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview?tabs=csharp) with [Cosmos DB binding](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2) and [Event Hubs trigger](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-event-hubs-trigger?tabs=csharp) that consumes all *events* produced by Saga participants, raises [external events](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-external-events?tabs=csharp) for orchestrator instances and persists the events on Cosmos DB.

* **Saga Status Checker:** [Azure Functions HTTP trigger](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook-trigger?tabs=csharp) that provides a schema as part of the HTTP response with the saga status (e.g. *Pending*, *Finished* and *Failed*) and the saga orchestrator runtime status (e.g. *Running* and *Completed*).
