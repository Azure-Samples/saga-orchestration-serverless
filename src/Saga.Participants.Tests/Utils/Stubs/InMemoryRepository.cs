using Saga.Common.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saga.Participants.Tests.Utils.Stubs
{
    public class InMemoryRepository<T> : IRepository<T>
    {
        public List<T> Items { get; set; }

        public InMemoryRepository()
        {
            Items = new List<T>();
        }

        public Task AddAsync(T entity)
        {
            Items.Add(entity);
            return Task.CompletedTask;
        }
    }
}
