# Architecture: Alternatives & Considerations

The Saga pattern can also be implemented through a [choreography](https://chrisrichardson.net/post/sagas/2019/08/04/developing-sagas-part-2.html) approach. In choreography, saga participants exchange *commands* and *events* without an orchestrator that coordinates the workflow. Both orchestration and choreography approaches have benefits and drawbacks:

| Saga | Benefits                   | Drawbacks                  |
| ------------- | -------------------------- | -------------------------- |
| Orchestration | <ol><li>Suited for complex Saga workflows that require large number of saga participants or new participants added over time</li><li>Does not introduce cyclic dependencies as the orchestrator depends on the saga participants but not vice versa</li><li>Less coupling, as saga participants don't need to know about commands that need to be produced for other participants</li><li>Clear separation of concerns that simplifies the business logic</li></ol> | <ol><li>Additional design complexity that requires an implementation of a coordination logic</li><li>Additional point of failure as it manages the complete workflow</li></ol> |
| Choreography  | <ol><li>Suited for simple Saga workflows that require few saga participants as there is no need to designing an implementation of a coordination logic</li><li>Does not require additional service implementation and maintenance</li><li>Does not introduce a single point of failure since the responsibilities are distributed across the saga participants</li></ol>| <ol><li>Workflow can become confusing while adding new steps, as it is difficult to track which saga participants listen to which commands</li><li>Potential risk of adding cyclic dependency between saga participants as they have to consume to one another's commands</li><li>Integration testing tends to be hard as all services should be running in order to simulate a transaction</li></ol> |

## Considerations

### Data modeling

Modeling data on Cosmos DB or any other NoSQL service requires assuming some benefits and drawbacks depending on the strategy applied. 

The proposed solution is leveraging the [Database-per-microservice](https://docs.microsoft.com/en-us/dotnet/architecture/cloud-native/database-per-microservice) pattern, where each Saga participant manages your own data on isolated databases for decoupling. It implies that the data of a Saga participant cannot be accessed directly from other Saga participants neither the orchestrator.


The solution, in the Cosmos DB perspective, is designed as *collection-per-microservice*. This brings some benefits and drawbacks:

| Data approach           | Benefits | Tradeoffs |
|-------------------------|----------|-----------|
| Collection-per-microservice | <ol><li>Domain data is encapsulated within the saga participant</li><li>Data schema can evolve without directly impacting other saga participants</li><li>Each saga participant data store can independently scale</li><li>A data store failure in one saga participant won't directly impact other participants</li></ol>   | <ol><li>Creation of queries that join data in multiple collections has potential to become complex and impact performance exponentially as the data volume increase over time</li><li>Cosmos DB has a limitation on Request Units (RUs) that impacts the max number of collections (more info [here](https://docs.microsoft.com/en-us/azure/cosmos-db/concepts-limits)) so depending on the number of saga participants it can be a limitation</li><li>More collections means increase of costs, as it will demand more RUs </li></ol>      |

To address the tradeoff related to the creation of complex queries, the solution is leveraging the [denormalization](https://docs.microsoft.com/en-us/azure/cosmos-db/how-to-model-partition-example#v2-introducing-denormalization-to-optimize-read-queries) approach to optimize read queries by creating an additional collection that consolidates the state results of all Saga participants. This approach drastically reduces complexity and improves performance for **observability** purposes.

![Cosmos DB data model](../img/cosmosdb-data-model.jpg)

Another approach for data modeling is defining a single collection shared across Saga participants. It will drastically reduce costs, but you will not have the decoupling benefits described above.

To decide how to provision throughput, containers and databases in Cosmos Db, we have to consider that Cosmos Db can provision throughput at two granularities:

* Containers: The throughput provisioned is exclusively for the container and it will consume all the throughput that is made available. Considering the internal distribution, the container distributes the throughput among its physical and logical partitions, and it cannot be specified the throughput for logical partitions. Because one or more logical partitions of a container are hosted by a physical partition, the physical partitions belong exclusively to the container and support the throughput provisioned on the container.
If the workload running on a logical partition consumes more than the throughput that was allocated to that logical partition, your operations get rate-limited. When rate-limiting occurs, you can either increase the provisioned throughput for the entire container or retry the operations.

* Databases: The throughput is shared across all containers in the database. It guarantees that receives the provisioned throughput for that database all the time. Because all containers within the database share the provisioned throughput, Azure Cosmos DB does not provide any predictable throughput dedication for any particular container in that database. The portion of the throughput that a specific container can receive depends on the number of containers, the choice of partition keys and the workload distribution across the logical partitions on the containers.
It is important to note that inside a Cosmos Db database, you can provision at most 25 containers that are grouped into a partition set and the database throughput is shared across these 25 containers. For every new container created beyond 25 containers, it creates a new partition set and the database throughput is split between the new partition sets. This approach reduces the throughput for a single partition set. A good approach is to keep 25 containers for a single database.

### Database Partitioning

This is the list of requests the proposed solution will have to expose:

* **[C]** Create/edit a Saga participant state per transaction ID
* **[Q1]** Retrieve the saga state per transaction ID
* **[Q2]** List each Saga participant state associated with a transaction ID

For the **[C]** item, a request is straightforward to implement as we just create or update an item on each Saga participant collection. The requests are nicely spread across all partitions with the `transaction ID` defined as the primary and partition key.

For the **[Q1]** item, retrieving a saga state is done by reading the corresponding `transaction ID` from the `transaction` collection. The requests are nicely spread across all partitions with the `transaction ID` defined as the primary and partition key.

For the **[Q2]** item, a request is straightforward to implement as all saga participant states are consolidated in the `saga` collection. The requests are nicely spread across all partitions with the `ID` as the primary key and `transaction ID` defined as the partition key.

For more information about strategies for data modeling and data partitioning, check the [How to model and partition data on Azure Cosmos DB using a real-world example](https://docs.microsoft.com/en-us/azure/cosmos-db/how-to-model-partition-example#identify-the-main-access-**patterns**) article.

### Message Broker Partitioning

Event Hubs provide message streaming through a partitioned consumer pattern in which each consumer only reads a specific subset, or partition, of the message stream. A partition is an ordered sequence of events that is held in an event hub. As newer events arrive, they are added to the end of this sequence. A partition can be thought of as a "commit log". 

The `transaction ID` field was chosen to guarantee that all messages related to a given transaction will be processed in sequence by each service. Since each service has its own dedicated inbox Event Hub, we didn't need to create different consumer groups for each service.

For additional details, please check [Event Hubs documentation](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-features#partitions).

### Scalability

Each Saga participant instance is backed by a single [Event Processor Host (EPH)](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-event-processor-host); an intelligent consumer agent that simplifies the management of checkpointing, leasing, and parallel event readers. The [Event Hubs trigger](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-event-hubs-trigger?tabs=csharp) ensures that only one EPH instance can get a lease on a given partition. For a better understanding, consider the following scenario of an Event Hub for a Saga participant:

* `N` partitions
* `W` commands distributed evenly across all partitions = `W/N` messages in each partition

Only one instance of the Saga participant function is created when the function starts; consider as `SagaParticipant-0` instance. The `SagaParticipant-0` has a single instance of the EPH that holds a lease on all `N` partitions, reading commands from partitions 0-(N-1). Then the following scenarios can occur:

| Scenario                | Description |
|-------------------------|-------------|
| No need for new Saga participant instance | `SagaParticipant-0` is able to process all `W` commands before the Functions scaling logic takes effect.     |
| A new Saga participant instance is added | If the Functions scaling logic determines that `SagaParticipant-0` has more messages than it can process, it creates a new function app instance (`SagaParticipant-1`) and it associates a new instance of the EPH. As the underlying Event Hubs detects that a new host instance is trying to read messages, it distributes the load across its partitions. For example, partitions 0-4 may be assigned to `SagaParticipant-0` and partitions 5-9 to `SagaParticipant-1`.|
| More Saga participant instances are added | If the Functions scaling logic determines that both `SagaParticipant-0` and `SagaParticipant-1` have more messages than they can process, new `SagaParticipant-Y` instances are created until reaching the state where `Y` is greater than `N` Event Hub partitions. In this scaling scenario, Event Hubs again load balances the partitions across the instances `SagaParticipant-0` to `SagaParticipant-(N-1)`.|

> Note: The same scaling mechanism for saga participants is used  for `Saga Event Processor` service, that also leverages the Event Hubs trigger binding.

The `Saga Orchestrator`, `Command Producer Activity` and `Saga Orchestrator Activity` functions are triggered by an internal queues in Azure Functions, known as **control queues**, that contain a variety of orchestration lifecycle message types. Orchestrator and activity instances are stateful singletons, so messages are load-balanced across the control queues instead of using a competing consumer model to distribute load across VMs. The Durable Functions task implements a random exponential back-off algorithm to reduce the effect of idle-queue polling on storage transaction costs. When a message is found, the runtime immediately checks for another message; when no message is found, it waits for a period of time before trying again. After subsequent failed attempts to get a queue message, the wait time continues to increase until it reaches the maximum wait time, which defaults to 30 seconds. For more info about the scaling mechanism, check the [Performance and scale in Durable Functions (Azure Functions)](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-perf-and-scale) documentation.

### Availability

* Azure Functions availability will vary according to the hosting plan decision: [Dedicated (App Service)](https://docs.microsoft.com/en-us/azure/azure-functions/functions-scale#app-service-plan), [Consumption](https://docs.microsoft.com/en-us/azure/azure-functions/functions-scale#consumption-plan) and [Premium](https://docs.microsoft.com/en-us/azure/azure-functions/functions-scale#premium-plan) plans.
* For Event Hubs, see the [Availability and consistency in Event Hubs](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-availability-and-consistency?tabs=latest) documentation.

### Security

* The solution leverages the default app settings mechanism provided by Azure Functions. To increase security on secrets management, it's recommended the use of [Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/basic-concepts). This topic shows you how to work with secrets from Azure Key Vault in your App Service or Azure Functions application without requiring any code changes: [Use Key Vault references for App Service and Azure Functions](https://docs.microsoft.com/en-us/azure/app-service/app-service-key-vault-references).

* The HTTP endpoints provided by `Saga Client` and `Saga Status Checker` functions must have a set of security improvements for production scenarios. To fully secure your function endpoints in production, you should consider implementing one of the following function app-level security options explained in the [Secure an HTTP endpoint in production](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook-trigger?tabs=csharp#secure-an-http-endpoint-in-production) documentation.

* Encryption at rest is applied automatically on new Cosmos DB databases. For more information about the security mechanisms applied on Cosmos DB, see the [Security in Azure Cosmos DB - overview](https://docs.microsoft.com/en-us/azure/cosmos-db/database-security) documentation.

* The solution does not provide role-based access control (RBAC) for fine-grained control over a client's access to resources on Event Hubs. It's recommended defining RBAC for production scenarios, see the [Authorize access to Azure Event Hubs](https://docs.microsoft.com/en-us/azure/event-hubs/authorize-access-event-hubs) documentation.

### Limitations

* See the [Azure Functions service limits](https://docs.microsoft.com/en-us/azure/azure-functions/functions-scale#service-limits) for scaling limitations on different billing models.
* See the [Cosmos DB storage and throughput](https://docs.microsoft.com/en-us/azure/cosmos-db/concepts-limits) for service quotas.
* See the [Event Hubs quotas and limits](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-quotas) per service tier.
