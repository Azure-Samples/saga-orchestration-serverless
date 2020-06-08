using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using Saga.Functions.Services.Activities;
using Saga.Orchestration.Models;
using Saga.Orchestration.Models.Transaction;
using Xunit;

namespace Saga.Functions.Tests.Services.Activities
{
    public class OrchestratorActivityTests : OrchestratorActivityTestsBase
    {
        [Fact]
        public async Task Pending_saga_state_should_be_persisted_on_database()
        {
            var documentCollectorMock = new Mock<IAsyncCollector<TransactionItem>>();
            var documentClientMock = new Mock<IDocumentClient>();

            var item = new TransactionItem
            {
                Id = Guid.NewGuid().ToString(),
                AccountFromId = Guid.NewGuid().ToString(),
                AccountToId = Guid.NewGuid().ToString(),
                Amount = 100.00M,
                State = nameof(SagaState.Pending)
            };

            documentCollectorMock
                .Setup(x => x.AddAsync(It.IsAny<TransactionItem>(), default))
                .Returns(
                    Task.CompletedTask
                );

            TransactionItem resultItem = await OrchestratorActivity
                .SagaOrchestratorActivity(item, documentCollectorMock.Object, documentClientMock.Object);

            Assert.Equal(item.Id, resultItem.Id);
        }

        [Theory]
        [MemberData(nameof(OrchestratorActivityInputData))]
        public async Task Saga_states_should_be_updated_on_database(TransactionItem item, TransactionItem newItem)
        {
            var documentCollectorMock = new Mock<IAsyncCollector<TransactionItem>>();
            var documentClientMock = new Mock<IDocumentClient>();

            var documents = new List<Document>
            {
                new Document
                {
                    Id = item.Id
                }
            };

            documentClientMock
                .Setup(x => x.CreateDocumentAsync(It.IsAny<Uri>(),
                        item,
                        It.IsAny<RequestOptions>(),
                        It.IsAny<bool>(),
                        default));

            documentClientMock
                .Setup(x => x.CreateDocumentQuery(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
                .Returns((IOrderedQueryable<Document>)documents.AsQueryable());

            TransactionItem resultItem = await OrchestratorActivity
                .SagaOrchestratorActivity(newItem, documentCollectorMock.Object, documentClientMock.Object);

            Assert.NotNull(resultItem);
            Assert.Equal(newItem.Id, resultItem.Id);
            Assert.Equal(newItem.State, resultItem.State);
        }
    }
}
