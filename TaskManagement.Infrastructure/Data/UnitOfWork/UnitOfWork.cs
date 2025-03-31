using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using TaskManagement.Infrastructure.Data.Context;
using TaskManagement.Infrastructure.Data.Repositories.Interfaces;

namespace TaskManagement.Infrastructure.Data.UnitOfWork
{
    /// <summary>
    /// Implementation of Unit of Work pattern using Entity Framework Core
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TaskManagementDbContext _dbContext;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction _transaction;
        private bool _disposed;

        /// <summary>
        /// Gets the task repository
        /// </summary>
        public ITaskRepository TaskRepository { get; }

        /// <summary>
        /// Creates a new UnitOfWork
        /// </summary>
        /// <param name="dbContext">Database context</param>
        /// <param name="logger">Logger</param>
        /// <param name="taskRepository">Task repository</param>
        public UnitOfWork(
            TaskManagementDbContext dbContext,
            ILogger<UnitOfWork> logger,
            ITaskRepository taskRepository)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            TaskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        }

        /// <inheritdoc />
        public async Task BeginTransactionAsync()
        {
            _logger.LogDebug("Beginning a new database transaction");
            _transaction = await _dbContext.Database.BeginTransactionAsync();
        }

        /// <inheritdoc />
        public async Task CommitTransactionAsync()
        {
            try
            {
                _logger.LogDebug("Committing database transaction");
                await _transaction?.CommitAsync();
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        /// <inheritdoc />
        public async Task RollbackTransactionAsync()
        {
            try
            {
                _logger.LogDebug("Rolling back database transaction");
                await _transaction?.RollbackAsync();
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        /// <inheritdoc />
        public async Task<int> SaveChangesAsync()
        {
            _logger.LogDebug("Saving changes to database");
            return await _dbContext.SaveChangesAsync();
        }

        private async Task DisposeTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the resources used by this unit of work
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                }

                _disposed = true;
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);

            Dispose(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously disposes the resources used by this unit of work
        /// </summary>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}