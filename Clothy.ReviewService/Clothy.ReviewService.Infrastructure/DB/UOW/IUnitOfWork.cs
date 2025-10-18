using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Clothy.ReviewService.Infrastructure.DB.UOW
{
    public interface IUnitOfWork : IDisposable
    {
        IClientSessionHandle Session { get; }
        Task StartTransactionAsync();
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task AbortAsync(CancellationToken cancellationToken = default);
    }
}
