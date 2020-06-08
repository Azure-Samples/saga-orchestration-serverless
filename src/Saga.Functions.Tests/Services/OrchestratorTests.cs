using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Saga.Common.Enums;
using Saga.Common.Messaging;
using Saga.Functions.Services;
using Saga.Functions.Tests.Utils;
using Saga.Orchestration.Models.Transaction;
using Xunit;

namespace Saga.Functions.Tests.Services
{
    public class OrchestratorTests : TestBase
    {
        [Fact]
        public async Task Saga_should_be_orchestrated_with_successful_workflow()
        {
            var contextMock = new Mock<IDurableOrchestrationContext>();
            var loggerMock = new Mock<ILogger>();

            contextMock.SetupCommandProducers();
            contextMock.SetupSagaPersisters();
            contextMock.SetupDurableOrchestratorInput();
            contextMock.SetupDurableOrchestratorExternalEvents();

            await Orchestrator.SagaOrchestrator(contextMock.Object, loggerMock.Object);

            contextMock
                .Verify(x => x.GetInput<TransactionItem>(), Times.Once());

            contextMock
               .Verify(x => x.WaitForExternalEvent<string>(nameof(Sources.Validator)), Times.Once());

            contextMock
               .Verify(x => x.WaitForExternalEvent<string>(nameof(Sources.Transfer)), Times.Once());

            contextMock
               .Verify(x => x.WaitForExternalEvent<string>(nameof(Sources.Receipt)), Times.Once());
        }

        [Fact]
        public void Saga_should_not_be_orchestrated_with_invalid_producers()
        {
            var contextMock = new Mock<IDurableOrchestrationContext>();
            var loggerMock = new Mock<ILogger>();

            var exception = Assert.ThrowsAsync<Exception>(async () =>
            {
                await Orchestrator.SagaOrchestrator(contextMock.Object, loggerMock.Object);
            });

            Assert.NotNull(exception);
        }

        private EventData CreateEventData(Command command)
        {
            string serializedMsg = JsonConvert.SerializeObject(command);
            byte[] messageBytes = Encoding.UTF8.GetBytes(serializedMsg);
            return new EventData(messageBytes);
        }
    }
}
