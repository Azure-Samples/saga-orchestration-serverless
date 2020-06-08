using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace Saga.Functions.Tests.Services
{
    public class SagaStatusCheckerServiceTestsBase : TestBase
    {
        protected Mock<IDurableOrchestrationClient> CreateDurableOrchestrationMock()
        {
            var clientMock = new Mock<IDurableOrchestrationClient>();

            var httpManagementPayloadContent = JsonConvert.SerializeObject(new
            {
                StatusQueryGetUri = string.Empty
            });

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(httpManagementPayloadContent),
                Headers =
                  {
                    RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(10))
                  }
            };

            clientMock
              .Setup(x => x.CreateCheckStatusResponse(It.IsAny<HttpRequestMessage>(), It.IsAny<string>(), false))
              .Returns(response);

            return clientMock;
        }

        protected HttpClient CreateValidHttpClient()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            var jsonContent = JsonConvert.SerializeObject(new DurableOrchestrationStatus
            {
                RuntimeStatus = OrchestrationRuntimeStatus.Completed
            });

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent)
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(response)
               .Verifiable();

            return new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test"),
            };
        }

        protected HttpClient CreateInvalidHttpClient()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            var jsonContent = JsonConvert.SerializeObject(new DurableOrchestrationStatus
            {
                RuntimeStatus = OrchestrationRuntimeStatus.Completed
            });

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent)
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
               )
               .Throws(new HttpRequestException())
               .Verifiable();

            return new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test"),
            };
        }
    }
}
