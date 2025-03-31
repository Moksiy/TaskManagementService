using System;
using System.Threading.Tasks;
using TaskManagement.Infrastructure.Data.Repositories.Interfaces;

namespace TaskManagement.Infrastructure.Data.UnitOfWork
{
    /// <summary>
    /// Interface for Unit of Work pattern implementation
    /// </summary>
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Gets the task repository
        /// </summary>
        ITaskRepository TaskRepository { get; }

        /// <summary>
        /// Begins a new transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits all changes made within the current transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back all changes made within the current transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}