using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.DAL.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task AddWithoutReturningAsync(T entity, CancellationToken cancelletionToken = default, IDbTransaction? transaction = null);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancelletionToken = default, IDbTransaction? transaction = null);
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancelletionToken = default, IDbTransaction? transaction = null);
        Task<Guid> AddAsync(T entity, CancellationToken cancelletionToken = default, IDbTransaction? transaction = null);
        Task<T?> UpdateAsync(T entity, CancellationToken cancelletionToken = default, IDbTransaction? transaction = null);
        Task<int> DeleteAsync(Guid id, CancellationToken cancelletionToken = default, IDbTransaction? transaction = null);
    }
}
