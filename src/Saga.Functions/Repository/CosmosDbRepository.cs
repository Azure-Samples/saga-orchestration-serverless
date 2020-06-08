using Microsoft.Azure.WebJobs;
using Saga.Common.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Saga.Functions.Repository
{
    public class CosmosDbRepository<T> : IRepository<T>
    {
        private readonly IAsyncCollector<T> stateCollector;

        public CosmosDbRepository(IAsyncCollector<T> stateCollector)
        {
            this.stateCollector = stateCollector;
        }
        public async Task AddAsync(T entity)
        {
            await stateCollector.AddAsync(entity);
        }
    }
}
