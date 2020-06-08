using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Saga.Common.Commands;
using Saga.Common.Enums;
using Saga.Common.Events;
using Saga.Orchestration.Models;
using Saga.Orchestration.Models.Activity;
using Saga.Orchestration.Models.Producer;
using Saga.Orchestration.Models.Transaction;
using Saga.Orchestration.Utils;

namespace Saga.Orchestration.Factories
{
    public class DurableOrchestrator
    {
        private readonly TimeSpan ValidatorTimeout = TimeoutUtils.FormatAccountsValidationTimeout();
        private readonly TimeSpan TransferTimeout = TimeoutUtils.FormatTransferTimeout();
        private readonly TimeSpan ReceiptTimeout = TimeoutUtils.FormatReceiptTimeout();
        private readonly Dictionary<string, Func<Task<ActivityResult<ProducerResult>>>> CommandProducers;
        private readonly Dictionary<string, Func<Task<bool>>> SagaStatePersisters;

        public DurableOrchestrator(
            Dictionary<string, Func<Task<ActivityResult<ProducerResult>>>> commandProducers,
            Dictionary<string, Func<Task<bool>>> sagaStatePersisters)
        {
            if (!ValidInputs(commandProducers, sagaStatePersisters))
            {
                throw new ArgumentException(ConstantStrings.InvalidParameters);
            }

            CommandProducers = commandProducers;
            SagaStatePersisters = sagaStatePersisters;
        }

        private bool ValidInputs(
            Dictionary<string, Func<Task<ActivityResult<ProducerResult>>>> commandProducers,
            Dictionary<string, Func<Task<bool>>> sagaStatePersisters)
        {
            return (commandProducers != null && commandProducers.Any())
                && (sagaStatePersisters != null && sagaStatePersisters.Any());
        }

        public async Task<SagaState> OrchestrateAsync(TransactionItem input, IDurableOrchestrationContext context, ILogger log)
        {
            bool sagaSatePersisted = await SagaStatePersisters[nameof(SagaState.Pending)]();

            if (!sagaSatePersisted)
            {
                return SagaState.Fail;
            }

            ActivityResult<ProducerResult> validateTransferCommandResult = await CommandProducers[nameof(ValidateTransferCommand)]();

            if (!validateTransferCommandResult.Valid)
            {
                await SagaStatePersisters[nameof(SagaState.Fail)]();
                return SagaState.Fail;
            }

            string validatorEventName = await DurableOrchestrationContextExtensions
              .WaitForExternalEventWithTimeout<string>(context, Sources.Validator, ValidatorTimeout);

            if (validatorEventName != nameof(TransferValidatedEvent))
            {
                log.LogError(string.Format(ConstantStrings.DurableOrchestratorErrorMessage, validatorEventName, Sources.Validator, context.InstanceId));
                await SagaStatePersisters[nameof(SagaState.Fail)]();

                return SagaState.Fail;
            }

            ActivityResult<ProducerResult> transferCommandResult = await CommandProducers[nameof(TransferCommand)]();

            if (!transferCommandResult.Valid)
            {
                await SagaStatePersisters[nameof(SagaState.Fail)]();
                return SagaState.Fail;
            }

            string transferEventName = await DurableOrchestrationContextExtensions
              .WaitForExternalEventWithTimeout<string>(context, Sources.Transfer, TransferTimeout);

            if (transferEventName != nameof(TransferSucceededEvent))
            {
                log.LogError(string.Format(ConstantStrings.DurableOrchestratorErrorMessage, transferEventName, Sources.Transfer, context.InstanceId));

                await SagaStatePersisters[nameof(SagaState.Fail)]();
                return SagaState.Fail;
            }

            ActivityResult<ProducerResult> issueReceiptCommandResult = await CommandProducers[nameof(IssueReceiptCommand)]();

            if (!issueReceiptCommandResult.Valid)
            {
                await SagaStatePersisters[nameof(SagaState.Fail)]();
                return SagaState.Fail;
            }

            string receiptEventName = await DurableOrchestrationContextExtensions
              .WaitForExternalEventWithTimeout<string>(context, Sources.Receipt, ReceiptTimeout);

            if (receiptEventName != nameof(ReceiptIssuedEvent))
            {
                log.LogError(string.Format(ConstantStrings.DurableOrchestratorErrorMessage, receiptEventName, Sources.Receipt, context.InstanceId));

                ActivityResult<ProducerResult> cancelTransferCommandResult = await CommandProducers[nameof(CancelTransferCommand)]();

                if (!cancelTransferCommandResult.Valid)
                {
                    await SagaStatePersisters[nameof(SagaState.Fail)]();
                    return SagaState.Fail;
                }

                string compensatedTransferEventName = await DurableOrchestrationContextExtensions
                    .WaitForExternalEventWithTimeout<string>(context, Sources.Transfer, TransferTimeout);

                if (compensatedTransferEventName == nameof(TransferCanceledEvent))
                {
                    await SagaStatePersisters[nameof(SagaState.Cancelled)]();
                    return SagaState.Cancelled;
                }

                await SagaStatePersisters[nameof(SagaState.Fail)]();
                return SagaState.Fail;
            }

            await SagaStatePersisters[nameof(SagaState.Success)]();
            log.LogInformation($@"Saga '{context.InstanceId}' finished successfully.");

            return SagaState.Success;
        }
    }
}
