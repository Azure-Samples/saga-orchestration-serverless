# Adding Saga participants in a workflow

Saga participants in the solution are implemented as Azure Functions for simplicity, but are not limited to Azure Functions framework. Saga participants can be implemented in any language, any framework and any platform if they are able to:

* Consume *commands* from Event Hubs
* Perform one or more operations that are part of the business logic
* Save the operation state on the database and ensure data consistency
* Produce *events* to Event Hubs

To add new Saga participants in the workflow, make sure you can address the following questions:

1. What *commands* and *events* the Saga orchestrator must produce/consume for the participant given successful and failed scenarios?
2. Which parts of the workflow the orchestrator must produce new *commands* and wait for *events*?
3. What *commands* the participant must be able to consume?
4. What *events* the participant must be able to produce given successful and failed scenarios?

## Creating Commands and Events

Let's consider a scenario where a new Saga participant called `Sample Service` needs to be added on the workflow. Commands and events models should be created on `Saga.Common` solution, under `Commands` and `Events` folders respectively.

Creating a command called `SampleCommand`:

```csharp
using Saga.Common.Messaging;

namespace Saga.Common.Commands
{
    public class SampleCommand : Command
    {
        // Additional properties here
    }
}
```

Creating a sample event called `SampleEvent`:

```csharp
using Saga.Common.Messaging;

namespace Saga.Common.Events
{
    public class SampleEvent : Event
    {
        // Additional properties here
    }
}
```

## Creating a Source

The `Sources` enum on `Saga.Common/Enums` folder contains a definition of all services that generates messages (*commands* and/or *events*). In this case, a new enum should be created for the Sample service:

```csharp
public enum Sources
{
    ...,
    Sample
}
```

## Creating a Saga participant

The solution has the following structure for each Saga participant:

| Folder/File                 | Description                                                                                                                  |
| --------------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| Factories                   | Folder that contains factories to simplify processing commands and producing event objects                                        |
| Models                   | Folder that contains the models that are part of the business logic                                        |
| Processors                  | Folder that contains *command processors* (i.e. classes with methods to process a specific *command*)                  |

In order to follow the same structure, you can create a `Sample` folder under the `Saga.Participants` solution:

```
📦Saga.Participants
    📂Sample
    ┣ 📂Factories
    ┃ ┗ 📜*.cs
    ┣ 📂Models
    ┃ ┗ 📜*.cs
    ┗ 📂Processors
      ┗ 📜*.cs
```

### Creating an Event Factory

The event factory contains methods to create all expected event objects within the participant domain. A `SampleServiceEventFactory` will be created under the `Saga.Participants/Sample/Factories` folder.

```csharp
public class SampleServiceEventFactory
{
    public static SampleEvent BuildSampleEvent(string transactionId)
    {
        return new SampleEvent
        {
            Header = new MessageHeader(transactionId, nameof(SampleEvent), Sources.Sample.ToString());
            // Additional properties here
        };
    }
}
```

### Creating a Repository model

A domain model called `Sample` will be used by the participant to persist data on the database (NoSQL or SQL): 

```csharp
using Newtonsoft.Json;
using Saga.Common.Messaging;
using Saga.Participants.Validator.Factories;

public class Sample
{
    [JsonProperty("id")]
    public string TransactionId { get; }

    // Additional properties here

    public Sample(string id)
    {
        TransactionId = id;
        // Additional properties here
    }

    public Event CreateSampleEvent()
    {
        return SampleServiceEventFactory.BuildSampleEvent(TransactionId);
    }
}
```

### Creating a Command Processor

Command processor is a class that contain methods to handle each type of command consumed by the participant. A `SampleCommandProcessor` class will be created to handle the `SampleCommand` message.

```csharp
public class SampleCommandProcessor : ICommandProcessor
{
    private readonly IMessageProducer eventProducer;
    private readonly IRepository<Sample> repository;

    public SampleCommandProcessor(IMessageProducer eventProducer, IRepository<Sample> repository)
    {
        this.eventProducer = eventProducer;
        this.repository = repository;
    }

    public async Task ProcessAsync(ICommandContainer commandContainer)
    {
        var sampleCommand = commandContainer.ParseCommand<SampleCommand>();

        // Your business logic here

        // Save the Sample object to database
        // await repository.AddAsync(sample);

        // Produce event
        // await eventProducer.ProduceAsync(sampleEvent);
    }
}
```

### Creating a Command Processor Factory

A Command Processor Factory is a class that maps all command processors expected for the Saga participant. A `SampleServiceCommandProcessorFactory` will be created and `SampleCommandProcessor` will be part of the processor map.

```csharp
public class SampleServiceCommandProcessorFactory
{
    public static IDictionary<string, ICommandProcessor> BuildProcessorMap(IMessageProducer eventProducer, IRepository<Sample> repository)
    {
        return new Dictionary<string, ICommandProcessor>
        {
            { nameof(SampleCommand), new SampleCommandProcessor(eventProducer, repository) }
        };
    }
}
```

### Creating the Saga Participant function

The `Saga.Functions` solution consolidates all Azure Functions for Saga participants. A `SampleService.cs` file that contains the `Sample` function will be created under `Saga.Functions/Services/Participants` folder.

```csharp
public static class SampleService
  {
    [FunctionName(nameof(Sample))]
    public static async Task Sample(
      [EventHubTrigger(@"%SampleEventHubName%", Connection = @"EventHubsNamespaceConnection")] EventData[] eventsData,
      [EventHub(@"%ReplyEventHubName%", Connection = @"EventHubsNamespaceConnection")]IAsyncCollector<EventData> eventCollector,
      [CosmosDB(
        databaseName: @"%CosmosDbDatabaseName%",
        collectionName: @"%CosmosDbSampleCollectionName%",
        ConnectionStringSetting = @"CosmosDbConnectionString")] IAsyncCollector<Sample> stateCollector,
        ILogger logger)
    {
      IMessageProducer eventProducer = new EventHubMessageProducer(eventCollector);
      IRepository<Sample> repository = new CosmosDbRepository<Sample>(stateCollector);

      var processors = SampleServiceCommandProcessorFactory.BuildProcessorMap(eventProducer, repository);
      var dispatcher = new CommandProcessorDispatcher(processors);

      foreach (EventData eventData in eventsData)
      {
        try
        {
            var commandContainer = CommandContainerFactory.BuildCommandContainer(eventData);
            await dispatcher.ProcessCommandAsync(commandContainer);
        }
        catch (Exception ex)
        {
          logger.LogError(ex, ex.Message);
        }
      }
    }
  }
```

> Note: The function consumes data from Event Hubs. Make sure you have the `SampleEventHubName` property definition on `local.settings.json` if running locally or on Functions environment app settings if running on Azure.

## Defining the Saga participant timeout (optional)

You can define a timeout on the orchestrator for scenarios when you expect to receive an event from the Saga participant in a period of time. If this is the case, create an utility method on `TimeoutUtils.cs`:

```csharp
public static TimeSpan FormatSampleTimeout() 
{
  int timeoutSeconds = int.Parse(Environment.GetEnvironmentVariable("SampleTimeoutSeconds"));
  return TimeSpan.FromSeconds(timeoutSeconds);
}
```

> Note: Make sure you have the `SampleTimeoutSeconds` property definition on `local.settings.json` if running locally or on Functions environment app settings if running on Azure.

Then create a variable on the `Orchestrator.cs` file that gets the timeout property from the app settings:

```csharp
private static readonly TimeSpan SampleTimeout = TimeoutUtils.FormatSampleTimeout();
```

It will be used later on waiting events from the `Sample` participant.

## Updating the orchestration workflow

### Creating a Command object

On the `CommandFactory` class located in the `Saga.Orchestration/Factories` folder, create a new method to return a new instance of the `SampleCommand`:

```csharp
public static SampleCommand BuildSampleCommand(TransactionItem item)
{
    return new SampleCommand
    {
        Header = BuildEventHeaderFromTransactionId(item.Id, nameof(ValidateTransferCommand)),
        // Additional properties here
    };
}
```

### Creating a Command Producer Activity function

On the `ProducerActivity` class located on `Saga.Functions/Services/Activities`, create a new Activity function called `SampleCommandProducerActivity` that will produce the `SampleCommand` to Event Hubs:

```csharp
[FunctionName(nameof(SampleCommandProducerActivity))]
public static async Task<ProducerResult> SampleCommandProducerActivity(
  [EventHub(@"%SampleEventHubName%", Connection = @"EventHubsNamespaceConnection")]IAsyncCollector<EventData> messagesCollector,
  [ActivityTrigger] SampleCommand command,
  ILogger log)
{
  Producer producer = new Producer(messagesCollector, log);
  return await producer.ProduceEventWithRetryAsync(command);
}
```

> Note: Make sure you have the `SampleEventHubName` property definition on `local.settings.json` if running locally or on Functions environment app settings if running on Azure.

### Creating a Command Producer

In the `ActivityFactory` class located on `Saga.Functions/Factories` folder, create a new method that creates a new instance of the `SampleCommand` and calls the `SampleCommandProducerActivity` activity function.

```csharp
public static async Task<ActivityResult<ProducerResult>> ProduceSampleCommandAsync(
    TransactionItem item, IDurableOrchestrationContext context, ILogger log)
{
    SampleCommand command = CommandFactory.BuildSampleCommand(item);
    string functionName = nameof(ProducerActivity.SampleCommandProducerActivity);
    return await RunProducerActivityAsync(functionName, command, context, log);
}
```

### Updating the Saga Orchestrator function

In the `Orchestrator` class located on `Saga.Functions/Services` folder, update the list of command producers to add the `ProduceSampleCommandAsync` method, associated with the `SampleCommand`.

```csharp
var commandProducers = new Dictionary<string, Func<Task<ActivityResult<ProducerResult>>>>
{
    ...,
    [nameof(SampleCommand)] = () => ActivityFactory.ProduceSampleCommandAsync(item, context, log),
};
```

### Updating the Orchestrator coordination logic

In the `OrchestrateAsync` method located on `Saga.Orchestration/Factories/DurableOrchestrator.cs`, define in which part of the workflow you want to introduce the `Sample` participant then call the command producer to produce the `SampleCommand` to Event Hubs:

```csharp
ActivityResult<ProducerResult> sampleCommandResult = await CommandProducers[nameof(Sample Command)]();

if (!sampleCommandResult.Valid)
{
    await SagaStatePersisters[nameof(SagaState.Fail)]();
    return SagaState.Fail;
}
```

Then wait for the event name from the `Sample` source:

```csharp
// If you don't need to handle timeouts, replace the code below by:
// string sampleEventName = await context.WaitForExternalEvent<string>(nameof(Sources.Sample));

string sampleEventName = await DurableOrchestrationContextExtensions
    .WaitForExternalEventWithTimeout<string>(context, Sources.Sample, SampleTimeout);
```

Then check if the response is the expected event name:

```csharp
if (sampleEventName != nameof(SampleEvent))
{
    string errorMessage = string.Format(ConstantStrings.DurableOrchestratorErrorMessage, sampleEventName, Sources.Sample, context.InstanceId);
    log.LogError(errorMessage);

    await SagaStatePersisters[nameof(SagaState.Fail)]();

    return SagaState.Fail;
}
```
