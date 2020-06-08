using System.Threading.Tasks;

namespace Saga.Common.Repository
{
    public interface IRepository<T>
    {
        Task AddAsync(T entity);
    }
}
