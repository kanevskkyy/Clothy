using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Clothy.ReviewService.Infrastructure.DB.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        private IMongoDatabase database;
        private IClientSessionHandle? activeSession;
        private bool disposed;

        public IClientSessionHandle Session => activeSession ?? throw new InvalidOperationException("Attention: Session has not been started.");

        public UnitOfWork(IMongoDatabase database)
        {
            this.database = database;
        }

        public async Task StartTransactionAsync()
        {
            activeSession = await database.Client.StartSessionAsync();
            activeSession.StartTransaction();
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (activeSession == null) throw new InvalidOperationException("Attention: No active session to commit!");

            await activeSession.CommitTransactionAsync(cancellationToken);
            DisposeSession();
        }

        public async Task AbortAsync(CancellationToken cancellationToken = default)
        {
            if (activeSession == null) return;

            await activeSession.AbortTransactionAsync(cancellationToken);
            DisposeSession();
        }

        private void DisposeSession()
        {
            activeSession?.Dispose();
            activeSession = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    DisposeSession();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
