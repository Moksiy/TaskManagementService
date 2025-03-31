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
        /// <returns>Task representing the asynchronous operation</returns>
        Task BeginTransactionAsync();

        /// <summary>
        /// Commits all changes made within the current transaction
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rolls back all changes made within the current transaction
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task RollbackTransactionAsync();

        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task<int> SaveChangesAsync();
    }
}