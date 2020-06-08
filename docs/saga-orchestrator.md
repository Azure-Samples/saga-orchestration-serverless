# Saga Orchestrator

Saga Orchestrator is responsible for managing the stateful workflows across Saga participants. Its design has the following premises:

* Should be completely decoupled from Saga participants, where the communication should occur through asynchronous messaging with a message streaming platform.
* Should contain only the coordination logic. Business logic should occur only on Saga participants.
* In the coordination logic, should only produce *commands* and wait for *events* from Saga participants to coordinate next steps decisions on the workflow.

To simplify the orchestrator implementation, it is recommended to have a well defined mapping of:

1. All *commands* and payloads the orchestrator should be able to produce
2. All *events* the orchestrator should wait from Saga participants
3. Successful and failed workflows

## Implementation details

The orchestrator function starts by getting the input object provided by the `Saga Client` function.

```csharp
TransactionItem item = context.GetInput<TransactionItem>();
```

Then maps:

* All *commands* and its respective command producer methods the orchestrator is allowed to execute, where each method is responsible for producing the command to a message streaming platform (e.g. Event Hubs)
* All possible saga states and its respective persister methods the orchestrator is allowed to execute, where each method is responsible for persisting the saga state on the database (e.g. Cosmos DB).

```csharp
var commandProducers = new Dictionary<string, Func<Task<ActivityResult<ProducerResult>>>>
{
    [nameof(ValidateTransferCommand)] = () => ActivityFactory.ProduceValidateTransferCommandAsync(item, context, log),
    [nameof(TransferCommand)] = () => ActivityFactory.ProduceTransferCommandAsync(item, context, log),
    [nameof(CancelTransferCommand)] = () => ActivityFactory.ProduceCancelTransferCommandAsync(item, context, log),
    [nameof(IssueReceiptCommand)] = () => ActivityFactory.ProduceIssueReceiptCommandAsync(item, context, log)
};

var sagaStatePersisters = new Dictionary<string, Func<Task<bool>>>
{
    [nameof(SagaState.Pending)] = () => SagaFactory.PersistSagaStateAsync(item, SagaState.Pending, context, log),
    [nameof(SagaState.Success)] = () => SagaFactory.PersistSagaStateAsync(item, SagaState.Success, context, log),
    [nameof(SagaState.Cancelled)] = () => SagaFactory.PersistSagaStateAsync(item, SagaState.Cancelled, context, log),
    [nameof(SagaState.Fail)] = () => SagaFactory.PersistSagaStateAsync(item, SagaState.Fail, context, log),
};
```

Then starts a new instance of the `DurableOrchestrator` and invoke the `OrchestrateAsync` method.

```csharp
try
{
    var orchestrator = new DurableOrchestrator(commandProducers, sagaStatePersisters);
    SagaState sagaState = await orchestrator.OrchestrateAsync(item, context, log);

    log.LogInformation($@"Saga state = {nameof(sagaState)} [{context.InstanceId}]");
}
catch (ArgumentException ex)
{
    log.LogError(ex.Message);
}
```

The `OrchestrateAsync` method starts by persisting the Saga state as `PENDING` on the database. If an error occurs on the persistence, the error is logged and the orchestrator will not start the coordination logic.

```csharp
bool sagaSatePersisted = await SagaStatePersisters[nameof(SagaState.Pending)]();

if (!sagaSatePersisted) {
    return SagaState.Fail;
}
```

The `ValidateTransferCommand` is produced to `Validator` event hub. If an error occurs while producing the `ValidateTransferCommand` to Event Hubs, the orchestrator persists the Saga state as `FAILED` on the database.

```csharp
ActivityResult<ProducerResult> validateTransferCommandResult = await CommandProducers[nameof(ValidateTransferCommand)]();

if (!validateTransferCommandResult.Valid)
{
    await SagaStatePersisters[nameof(SagaState.Fail)]();
    return SagaState.Fail;
}
```

Otherwise, the orchestrator waits for the *event* from the `Validator` saga participant up to a specified response time (defined on `ValidatorTimeout` constant).

```csharp
string validatorEventName = await DurableOrchestrationContextExtensions
    .WaitForExternalEventWithTimeout<string>(context, Sources.Validator, ValidatorTimeout);
```

In a successful scenario, the `Validator` participant should return a `TransferValidatedEvent`. Otherwise, two potential failures are covered:

1. An error occurred on the transfer validation operation and a failed *event* was produced.
2. The orchestrator didn't receive the *event* from the `Validator` participant in a specified response time.

If one of the potential failures occur, the orchestrator persists the Saga state as `FAILED` on the database.

```csharp
if (validatorEventName != nameof(TransferValidatedEvent))
{
    log.LogError($@"{validatorEventName} returned from {Sources.Validator.ToString()} (transaction id: {context.InstanceId})");
    await SagaStatePersisters[nameof(SagaState.Fail)]();

    return SagaState.Fail;
}
```

Then the workflow continues with similar steps:

1. Produce new *commands* to Event Hubs and validate if these *commands* were produced successfully
2. Wait for *events* from Saga participants and takes 3 possible actions:
   1. Move to the next step of the workflow if received a successful event
   2. Produce a new compensation *command* if received a failed event
   3. Persist the Saga state as `FAILED` on the database and finish the workflow

After completing a successful workflow, persist the Saga state as `SUCCESS` on the database and finish the workflow.

```csharp
await SagaStatePersisters[nameof(SagaState.Success)]();
log.LogInformation($@"Saga '{context.InstanceId}' finished successfully.");

return SagaState.Success;
```

> The complete orchestration source code can be found on `src/Sga.Orchestration/Services/Orchestrator.cs`.

## Creating unit tests

The `Saga.Orchestrator.Tests` solution leverages [xUnit.net](https://xunit.net/) and [Moq](https://github.com/moq/moq4) for the creation of unit tests.

### Testing a successful workflow

Let's consider you want to test a successful workflow given the following scenario:

* The orchestrator should produce `ValidateTransferCommand`, `TransferCommand` and `IssueReceiptCommand`.
* The orchestrator should receive `TransferValidatedEvent` from `Validator` participant, `TransferSucceededEvent` from `Transfer` participant and `ReceiptIssuedEvent` from `Receipt` participant.
* The orchestrator should persist the saga state as `Pending` and `Success`.

First, create a new instance of the `TransactionItem`, which is the orchestrator input:

```csharp
var item = new TransactionItem
{
    Id = Guid.NewGuid().ToString(),
    AccountFromId = Guid.NewGuid().ToString(),
    AccountToId = Guid.NewGuid().ToString(),
    Amount = 100.00M,
    State = SagaState.Pending.ToString()
};
```

Then define all *commands* and its respective command producer methods, as well as the expected saga state persisters:

```csharp
var commandProducers = new Dictionary<string, Func<Task<ActivityResult<ProducerResult>>>>
{
    [nameof(ValidateTransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
    {
        Item = new ProducerResult()
    }),
    [nameof(TransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
    {
        Item = new ProducerResult()
    }),
    [nameof(IssueReceiptCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
    {
        Item = new ProducerResult()
    })
};

 var sagaStatePersisters = new Dictionary<string, Func<Task<bool>>>
{
    [nameof(SagaState.Pending)] = () => Task.Run(() => true),
    [nameof(SagaState.Success)] = () => Task.Run(() => true),
};
```

Then two mocks are required:

* External events response from `IDurableOrchestrationContext`
* Logs from `ILogger`

```csharp
var loggerMock = new Mock<ILogger>();
var mockContext = new Mock<IDurableOrchestrationContext>();

mockContext
    .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Validator)))
    .ReturnsAsync(nameof(TransferValidatedEvent));

mockContext
    .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Transfer)))
    .ReturnsAsync(nameof(TransferSucceededEvent));

mockContext
    .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Receipt)))
    .ReturnsAsync(nameof(ReceiptIssuedEvent));
```

Then starts the orchestrator with `SagaState.Success` as the expected response:

```csharp
var orchestrator = new DurableOrchestrator(commandProducers, sagaStatePersisters);
SagaState sagaState = await orchestrator.OrchestrateAsync(item, mockContext.Object, loggerMock.Object);

Assert.Equal(SagaState.Success, sagaState);
```

### Testing a compensating workflow

Let's consider you want to test a workflow that requires compensating transactions given the following scenario:

* The orchestrator should produce `ValidateTransferCommand`, `TransferCommand`, `IssueReceiptCommand` and `CancelTransferCommand`.
* The orchestrator should receive `TransferValidatedEvent` from `Validator` participant, `TransferSucceededEvent` from `Transfer` participant, `OtherReasonReceiptFailedEvent` from `Receipt` participant and `TransferCanceledEvent` from the `Transfer` participant.
* The orchestrator should persist the saga state as `Pending` and `Cancelled`.

First, create a new instance of the `TransactionItem`, which is the orchestrator input:

```csharp
var item = new TransactionItem
{
    Id = Guid.NewGuid().ToString(),
    AccountFromId = Guid.NewGuid().ToString(),
    AccountToId = Guid.NewGuid().ToString(),
    Amount = 100.00M,
    State = SagaState.Pending.ToString()
};
```

Then define all *commands* and its respective command producer methods, as well as the expected saga state persisters:

```csharp
var commandProducers = new Dictionary<string, Func<Task<ActivityResult<ProducerResult>>>>
{
    [nameof(ValidateTransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
    {
        Item = new ProducerResult()
    }),
    [nameof(TransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
    {
        Item = new ProducerResult()
    }),
    [nameof(CancelTransferCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
    {
        Item = new ProducerResult()
    }),
    [nameof(IssueReceiptCommand)] = () => Task.Run(() => new ActivityResult<ProducerResult>
    {
        Item = new ProducerResult()
    })
};

var sagaStatePersisters = new Dictionary<string, Func<Task<bool>>>
{
    [nameof(SagaState.Pending)] = () => Task.Run(() => true),
    [nameof(SagaState.Cancelled)] = () => Task.Run(() => true)
};
```

Then create the mocks:

```csharp
var loggerMock = new Mock<ILogger>();
var mockContext = new Mock<IDurableOrchestrationContext>();

mockContext
    .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Validator)))
    .ReturnsAsync(nameof(TransferValidatedEvent));

mockContext
    .SetupSequence(x => x.WaitForExternalEvent<string>(nameof(Sources.Transfer)))
    .ReturnsAsync(nameof(TransferSucceededEvent))
    .ReturnsAsync(nameof(TransferCanceledEvent));

mockContext
    .Setup(x => x.WaitForExternalEvent<string>(nameof(Sources.Receipt)))
    .ReturnsAsync(nameof(OtherReasonReceiptFailedEvent));
```

Then starts the orchestrator with `SagaState.Cancelled` as the expected response:

```csharp
var orchestrator = new DurableOrchestrator(commandProducers, sagaStatePersisters);
SagaState sagaState = await orchestrator.OrchestrateAsync(item, mockContext.Object, loggerMock.Object);

Assert.Equal(SagaState.Cancelled, sagaState);
```
