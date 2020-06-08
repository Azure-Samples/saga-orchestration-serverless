//using SagaOrchestration.Models.Account;
//using SagaOrchestration.Models.Events;
//using SagaOrchestration.Models.Receipt;
//using SagaOrchestration.Models.Transfer;

namespace Saga.Functions.Tests
{
    public class DurableOrchestrationContextExtensionsTests : TestBase
    {
        //[Fact]
        //public async Task Trigger_account_event_should_return_valid_state()
        //{
        //  var mockContext = new Mock<IDurableOrchestrationContext>();

        //  mockContext
        //    .Setup(x => x.WaitForExternalEvent<string>(EventType.AccountsValidatorEvent.ToString()))
        //    .ReturnsAsync(AccountState.Valid.ToString());

        //  string state = await DurableOrchestrationContextExtensions
        //    .WaitForExternalEventWithTimeout<string>(mockContext.Object, EventType.AccountsValidatorEvent.ToString(), TimeSpan.FromSeconds(5));

        //  Assert.NotNull(state);
        //  Assert.NotEmpty(state);
        //  Assert.Equal(state, AccountState.Valid.ToString());
        //}

        //[Fact]
        //public async Task Trigger_account_event_should_return_invalid_state()
        //{
        //  var mockContext = new Mock<IDurableOrchestrationContext>();

        //  mockContext
        //    .Setup(x => x.WaitForExternalEvent<string>(EventType.AccountsValidatorEvent.ToString()))
        //    .ReturnsAsync(AccountState.Invalid.ToString());

        //  string state = await DurableOrchestrationContextExtensions
        //    .WaitForExternalEventWithTimeout<string>(mockContext.Object, EventType.AccountsValidatorEvent.ToString(), TimeSpan.FromSeconds(5));

        //  Assert.NotNull(state);
        //  Assert.NotEmpty(state);
        //  Assert.Equal(state, AccountState.Invalid.ToString());
        //}

        //[Fact]
        //public async Task Trigger_transfer_event_should_return_completed_state()
        //{
        //  var mockContext = new Mock<IDurableOrchestrationContext>();

        //  mockContext
        //    .Setup(x => x.WaitForExternalEvent<string>(EventType.TransferEvent.ToString()))
        //    .ReturnsAsync(TransferState.Completed.ToString());

        //  string state = await DurableOrchestrationContextExtensions
        //    .WaitForExternalEventWithTimeout<string>(mockContext.Object, EventType.TransferEvent.ToString(), TimeSpan.FromSeconds(5));

        //  Assert.NotNull(state);
        //  Assert.NotEmpty(state);
        //  Assert.Equal(state, TransferState.Completed.ToString());
        //}

        //[Fact]
        //public async Task Trigger_transfer_event_should_return_failed_state()
        //{
        //  var mockContext = new Mock<IDurableOrchestrationContext>();

        //  mockContext
        //    .Setup(x => x.WaitForExternalEvent<string>(EventType.TransferEvent.ToString()))
        //    .ReturnsAsync(TransferState.Failed.ToString());

        //  string state = await DurableOrchestrationContextExtensions
        //    .WaitForExternalEventWithTimeout<string>(mockContext.Object, EventType.TransferEvent.ToString(), TimeSpan.FromSeconds(5));

        //  Assert.NotNull(state);
        //  Assert.NotEmpty(state);
        //  Assert.Equal(state, TransferState.Failed.ToString());
        //}

        //[Fact]
        //public async Task Trigger_receipt_event_should_return_created_state()
        //{
        //  var mockContext = new Mock<IDurableOrchestrationContext>();

        //  mockContext
        //    .Setup(x => x.WaitForExternalEvent<string>(EventType.ReceiptEvent.ToString()))
        //    .ReturnsAsync(ReceiptState.Created.ToString());

        //  string state = await DurableOrchestrationContextExtensions
        //    .WaitForExternalEventWithTimeout<string>(mockContext.Object, EventType.ReceiptEvent.ToString(), TimeSpan.FromSeconds(5));

        //  Assert.NotNull(state);
        //  Assert.NotEmpty(state);
        //  Assert.Equal(state, ReceiptState.Created.ToString());
        //}

        //[Fact]
        //public async Task Trigger_receipt_event_should_return_failed_state()
        //{
        //  var mockContext = new Mock<IDurableOrchestrationContext>();

        //  mockContext
        //    .Setup(x => x.WaitForExternalEvent<string>(EventType.ReceiptEvent.ToString()))
        //    .ReturnsAsync(ReceiptState.Failed.ToString());

        //  string state = await DurableOrchestrationContextExtensions
        //    .WaitForExternalEventWithTimeout<string>(mockContext.Object, EventType.ReceiptEvent.ToString(), TimeSpan.FromSeconds(5));

        //  Assert.NotNull(state);
        //  Assert.NotEmpty(state);
        //  Assert.Equal(state, ReceiptState.Failed.ToString());
        //}

        //[Fact]
        //public async Task Trigger_unexpected_event_should_be_invalid()
        //{
        //  var mockContext = new Mock<IDurableOrchestrationContext>();

        //  mockContext
        //    .Setup(x => x.WaitForExternalEvent<string>(EventType.AccountsValidatorEvent.ToString()))
        //    .ReturnsAsync(AccountState.Valid.ToString());

        //  string state = await DurableOrchestrationContextExtensions
        //    .WaitForExternalEventWithTimeout<string>(mockContext.Object, EventType.TransferEvent.ToString(), TimeSpan.FromSeconds(5));

        //  Assert.Null(state);
        //}

        //[Fact]
        //public async Task Call_null_event_should_be_invalid()
        //{
        //  var mockContext = new Mock<IDurableOrchestrationContext>();

        //  mockContext
        //    .Setup(x => x.WaitForExternalEvent<string>(null))
        //    .ReturnsAsync((string)null);

        //  string state = await DurableOrchestrationContextExtensions
        //    .WaitForExternalEventWithTimeout<string>(mockContext.Object, EventType.AccountsValidatorEvent.ToString(), TimeSpan.FromSeconds(30));

        //  Assert.Null(state);
        //}


    }
}
