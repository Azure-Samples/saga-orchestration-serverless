# Architecture: Additional patterns applied

* [Async HTTP APIs](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview?tabs=csharp#async-http): Designing an API that provides s synchronous HTTP response in the context of Saga is not best approach, as Saga is designed for long-lived transactions (i.e. long-running operations). The Async HTTP API pattern is used to address this problem, where `Saga Client` triggers the long-running action and redirects the client to a status endpoint that allows the client to poll it to know when the transaction is finished.

* [Retry](https://docs.microsoft.com/en-us/azure/architecture/patterns/retry): Pattern used on producing messages to Event Hubs and [activities](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-types-features-overview#activity-functions) calls from the orchestrator.
  
* [Circuit Breaker](https://docs.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker): Pattern integrated with Retry on producing messages to Event Hubs.

* [Database-per-microservice](https://docs.microsoft.com/en-us/dotnet/architecture/cloud-native/database-per-microservice): Pattern used to ensure that each Saga participant manages your own data decoupled from other participants.
