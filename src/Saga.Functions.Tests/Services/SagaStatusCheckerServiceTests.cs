using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Saga.Functions.Models;
using Saga.Functions.Services;
using Saga.Orchestration.Models;
using Saga.Orchestration.Models.Transaction;
using Xunit;

namespace Saga.Functions.Tests.Services
{
    public class SagaStatusCheckerServiceTests : SagaStatusCheckerServiceTestsBase
    {
        [Fact]
        public async Task Request_should_return_saga_status()
        {
            var transactionId = Guid.NewGuid().ToString();
            Mock<IDurableOrchestrationClient> clientMock = CreateDurableOrchestrationMock();
            HttpClient httpClient = CreateValidHttpClient();
            var loggerMock = new Mock<ILogger>();

            var documentClientMock = new Mock<IDocumentClient>();

            var documents = new List<TransactionItem>
            {
                new TransactionItem
                {
                    Id = transactionId,
                    AccountFromId = Guid.NewGuid().ToString(),
                    AccountToId = Guid.NewGuid().ToString(),
                    Amount = 100.00M,
                    State = nameof(SagaState.Pending)
                }
            }
            .AsQueryable() as IOrderedQueryable<TransactionItem>;

            documentClientMock
                .Setup(x => x.CreateDocumentQuery<TransactionItem>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(documents);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($@"http://localhost:7071/api/saga/state/{transactionId}"),
            };

            var sagaStatusService = new SagaStatusCheckerService(httpClient);
            var result = await sagaStatusService
                .SagaStatusChecker(request, transactionId, clientMock.Object, documentClientMock.Object, loggerMock.Object);

            var okObjectResult = result as OkObjectResult;
            Assert.NotNull(okObjectResult);

            var sagaStatusResponse = okObjectResult.Value as SagaStatusResponse;

            Assert.NotNull(sagaStatusResponse);
            Assert.NotNull(sagaStatusResponse.Status);
            Assert.NotNull(sagaStatusResponse.Status.SagaState);
            Assert.NotEmpty(sagaStatusResponse.Status.SagaState);
            Assert.NotNull(sagaStatusResponse.Status.OrchestrationEngineState);
            Assert.NotEmpty(sagaStatusResponse.Status.OrchestrationEngineState);
        }

        [Fact]
        public async Task Request_should_not_return_saga_status()
        {
            Mock<IDurableOrchestrationClient> clientMock = CreateDurableOrchestrationMock();
            HttpClient httpClient = CreateValidHttpClient();
            var documentClientMock = new Mock<IDocumentClient>();
            var loggerMock = new Mock<ILogger>();

            var transactionId = Guid.NewGuid().ToString();
            var documents = new List<TransactionItem>();

            documentClientMock
                .Setup(x => x.CreateDocumentQuery<TransactionItem>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns(documents.AsQueryable() as IOrderedQueryable<TransactionItem>);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($@"http://localhost:7071/api/saga/state/{transactionId}"),
            };

            var sagaStatusService = new SagaStatusCheckerService(httpClient);
            var result = await sagaStatusService
                .SagaStatusChecker(request, transactionId, clientMock.Object, documentClientMock.Object, loggerMock.Object);

            Assert.NotNull(result as NotFoundObjectResult);
        }
    }
}
