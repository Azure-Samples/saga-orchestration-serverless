using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Saga.Functions.Tests
{
    public class InMemoryAsyncCollector<T> : IAsyncCollector<T>
    {
        public IList<T> Items { get; }

        public InMemoryAsyncCollector()
        {
            Items = new List<T>();
        }

        public Task AddAsync(T item, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) throw new InvalidOperationException("cancellationToken was triggered");

            Items.Add(item);

            return Task.CompletedTask;
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) throw new InvalidOperationException("cancellationToken was triggered");

            return Task.CompletedTask;
        }

    }
}
