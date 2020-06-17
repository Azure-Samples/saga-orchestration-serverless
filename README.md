---
page_type: sample
languages:
- csharp
products:
- azure
- terraform
- azure-functions
- azure-event-hubs
- azure-cosmosdb
description: "Orchestration-based Saga implementation reference in a Serverless architecture on Azure"
---

# Orchestration-based Saga on Serverless 
![saga-build](https://github.com/Azure-Samples/saga-orchestration-serverless/workflows/saga-build/badge.svg)

Contoso Bank is building a new payment platform leveraging the development of microservices to rapidly offer new features in the market, where legacy and new applications coexist. Operations are now distributed across applications and databases, and Contoso needs a new architecture and implementation design to ensure data consistency on financial transactions.

The traditional [ACID](https://en.wikipedia.org/wiki/ACID) approach is not suited anymore for Contoso Bank as the data of operations are now spanned into isolated databases. Instead of ACID transactions, a [Saga](https://www.cs.cornell.edu/andru/cs711/2002fa/reading/sagas.pdf) addresses the challenge by coordinating a workflow through a message-driven sequence of local transactions to ensure data consistency.

## About the solution

The solution simulates a money transfer scenario, where an amount is transferred between bank accounts through credit/debit operations and an operation receipt is generated for the requester. It is a Saga pattern implementation reference through an orchestration approach in a serverless architecture on Azure. The solution leverages [Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview) for the implementation of Saga participants, [Azure Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview?tabs=csharp) for the implementation of the Saga orchestrator, [Azure Event Hubs](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-about) as the data streaming platform and [Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/introduction) as the database service.

The implementation reference addresses the following challenges and concerns from Contoso Bank:

* **Developer experience:** A solution that allows developers focus only on the business logic of the Saga participants and simplify the implementation of stateful workflows on the Saga orchestrator. The proposed solution leverages the Azure Functions programming model, reducing the overhead on state management, checkpointing (mechanism that updates the offset of a messaging partition when a consumer service processes a message) and restarts in case of failures.
  
* **Resiliency:** A solution capable of handling a set of potential transient failures (e.g. operation retries on databases and message streaming platforms, timeout handling). The proposed solution applies a set of design patterns (e.g. Retry and Circuit Breaker) on operations with Event Hubs and Cosmos DB, as well as timeout handling on the production of *commands* and *events*.

* **Idempotency:** A solution where each Saga participant can execute multiple times and provide the same result to reduce side effects, as well as to ensure data consistency. The proposed solution relies on validations on Cosmos DB for idempotency, making sure there is no duplication on the transaction state and no duplication on the creation of *events*.
  
* **Observability:** A solution that is capable of monitoring and tracking the Saga workflow states per transaction. The proposed solution leverages Cosmos DB collections that allows the track of the workflow by applying a single query.

## Potential use cases

These other uses cases have similar design patterns:

* Settlement transactions
* Order services (e.g. e-commerce, food delivery, flight/hotel/taxi booking)

## Architecture

![Architecture Overview](./docs/img/architecture-overview.jpg)

Check the following sections about the core components of the solution, workflows and design decisions:

* [Architecture: Core Components](docs/architecture/components.md)
* [Architecture: Workflows](./docs/architecture/workflows.md)
* [Architecture: Additional patterns applied](./docs/architecture/additional-patterns.md)
* [Architecture: Alternatives & Considerations](./docs/architecture/alternatives-and-considerations.md)

## Maturity Level

These are the top skills required for Solution Architects and Developer Leads that will reuse the proposed solution or parts of the implementation as reference:

* **Architectures:** Microservices, Event-driven and Serverless
* **Design Patterns:** Saga, Database-per-microservice, Async HTTP API, Retry, Circuit Breaker
* **Azure services:** Azure Functions (including Event Hubs + Cosmos DB bindings and Durable Functions extension), Event Hubs, Cosmos DB, Azure Storage
* **Programming Language:** C#
* **Additional concepts:** Infrastructure as Code, RESTful APIs, idempotency
  
## Repository Structure

Outline the file contents of the repository. It helps users navigate the codebase, build configuration and any related assets.

| File/folder       | Description                                |
|-------------------|--------------------------------------------|
| `docs`             | Contains the project docs and images                        |
| `src`             | Contains the sample source code                       |
| `.gitignore`      | Defines what files to be igored at commit time      |
| `CODE_OF_CONDUCT.md`         | Microsoft Open Source Code of Conduct                |
| `CONTRIBUTING.md` | Guidelines for contributing to the sample |
| `LICENSE`         | The license defined for the sample               |
| `README.md`       | Project wiki                          |
| `SECURITY`         | Security guidelines for the sample                |

The `src` folder contains a set of .NET Core solutions:

| Project                | Description  |
|-------------------------|--------|
| Saga.Common | Solution that contains a set of reusable abstractions, models, *commands* and *events* objects and utilities |
| Saga.Common.Tests | Solution that contains all unit tests for common utilities and processors |
| Saga.Functions | Solution that contains all Azure Functions |
| Saga.Functions.Tests | Solution that contains all Azure Functions unit tests |
| Saga.Orchestration | Solution that contains domain models and the coordination logic for the Saga Orchestrator |
| Saga.Orchestration.Tests | Solution that contains unit tests for the Saga Orchestrator |
| Saga.Participants | Solution that contains domain models and business logic of all Saga participants |
| Saga.Participants.Tests | Solution that contains unit tests of all Saga participants |

## Prerequisites

* An Azure account with an active subscription. [Create an account for free](https://azure.microsoft.com/en-us/free/?ref=microsoft.com&utm_source=microsoft.com&utm_medium=docs&utm_campaign=visualstudio).
* [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)
* [Terraform](https://learn.hashicorp.com/terraform/azure/install_az) 0.2.6 or later
* [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=linux%2Ccsharp%2Cbash) v3.0 or later
* [.NET Core](https://dotnet.microsoft.com/download) 3.1 or later

If [Visual Studio Code](https://code.visualstudio.com/) is your platform preference for development:

* [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
* [Azure Functions extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions)

If [Visual Studio](https://azure.microsoft.com/en-us/downloads/) is your platform preference for development:

* Visual Studio 2019 with [Azure development](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-your-first-function-visual-studio#prerequisites) workload

## Getting Started

### 1. Create the infrastructure

Follow the instructions on [Create Infrastructure as Code](docs/infrastructure-as-code.md) to create a new GitHub Actions pipeline that creates the required infrastructure on Azure through Terraform.

### 2. Creating the application settings

The solution requires a set of environment variables to be configured on `Saga.Functions` solution for running the project locally. Follow the [Configuring the Azure Functions Application Settings](./docs/functions-app-settings.md) instructions to create a `local.settings.json`. Make sure the settings are configured before proceeding to the next step.

### 4. Running unit tests

Run the following command on test projects:

```sh
dotnet test
```

### 5. Running the solution

For running the solution through command line, make sure you're on the `src/Saga.Functions` directory then run the following command:

```sh
func host start
```

The solution provides a Swagger UI on `http://localhost:<port>/api/swagger/ui`. 

![Swagger UI](./docs/img/swagger-ui.jpg)

You can start a new HTTP request on `/api/saga/start` route by providing the following json as part of the body content:

```json
{
    "accountFromId": "<random ID>",
    "accountToId": "<random ID>",
    "amount": 100.00
}
```

After executing the request, it's expected to receive the transaction ID as part of the HTTP response.

![Saga Starter request](./docs/img/swagger-saga-starter.jpg)

You can check the documents created on Cosmos DB. It is expected to have the following document structure on the `orchestrator` collection:

```json
{
  "accountFromId": "<random bank account ID>",
  "accountToId": "<random bank account ID>",
  "amount": 100,
  "id": "<random transaction ID>",
  "type": "Default",
  "state": "Success",
  ...
}
```

For observability, check the `saga` collection that contains all events processed by the orchestrator.  It is expected to have the following structure:

```json
{
  "transactionId": "<random transaction ID>",
  "source": "<Validator, Transfer or Receipt>",
  "messageType": "<event name>",
  "creationDate": "<event creation date>",
  ...
}
```

## Next Steps

* [Understanding the Saga Orchestrator](docs/saga-orchestrator.md)
* [Adding new Saga participants in a workflow](./docs/add-saga-participants.md)
* [Commands and Events Contracts](./docs/commands-events.md)

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
