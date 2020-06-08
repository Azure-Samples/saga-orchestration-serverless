using System;
using Saga.Common.Commands;
using Saga.Common.Enums;
using Saga.Orchestration.Factories;
using Saga.Orchestration.Models;
using Saga.Orchestration.Models.Transaction;
using Xunit;

namespace Saga.Orchestration.Tests
{
    public class OrchestratorCommandFactoryTests : TestBase
    {
        [Fact]
        public void Validate_command_should_be_built_with_valid_payload()
        {
            var item = new TransactionItem
            {
                Id = Guid.NewGuid().ToString(),
                AccountFromId = Guid.NewGuid().ToString(),
                AccountToId = Guid.NewGuid().ToString(),
                Amount = 100.00M,
                State = SagaState.Pending.ToString()
            };

            ValidateTransferCommand validateTransferCommand = CommandFactory.BuildValidateTransferCommand(item);

            Assert.NotNull(validateTransferCommand);
            Assert.Equal(validateTransferCommand.Header.TransactionId, item.Id);
            Assert.Equal(nameof(ValidateTransferCommand), validateTransferCommand.Header.MessageType);
            Assert.Equal(nameof(Sources.Orchestrator), validateTransferCommand.Header.Source);
        }

        [Fact]
        public void Transfer_command_should_be_built_with_valid_payload()
        {
            var item = new TransactionItem
            {
                Id = Guid.NewGuid().ToString(),
                AccountFromId = Guid.NewGuid().ToString(),
                AccountToId = Guid.NewGuid().ToString(),
                Amount = 100.00M,
                State = SagaState.Pending.ToString()
            };

            TransferCommand transferCommand = CommandFactory.BuildTransferCommand(item);

            Assert.NotNull(transferCommand);
            Assert.Equal(transferCommand.Header.TransactionId, item.Id);
            Assert.Equal(nameof(TransferCommand), transferCommand.Header.MessageType);
            Assert.Equal(nameof(Sources.Orchestrator), transferCommand.Header.Source);
        }

        [Fact]
        public void Receipt_command_should_be_built_with_valid_payload()
        {
            var item = new TransactionItem
            {
                Id = Guid.NewGuid().ToString(),
                AccountFromId = Guid.NewGuid().ToString(),
                AccountToId = Guid.NewGuid().ToString(),
                Amount = 100.00M,
                State = SagaState.Pending.ToString()
            };

            IssueReceiptCommand issueReceiptCommand = CommandFactory.BuildIssueReceiptCommand(item);

            Assert.NotNull(issueReceiptCommand);
            Assert.Equal(issueReceiptCommand.Header.TransactionId, item.Id);
            Assert.Equal(nameof(IssueReceiptCommand), issueReceiptCommand.Header.MessageType);
            Assert.Equal(nameof(Sources.Orchestrator), issueReceiptCommand.Header.Source);
        }
    }
}
