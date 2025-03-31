namespace TaskManagement.Infrastructure.Data.Repositories.Interfaces
{
    /// <summary>
    /// Interface for task repository
    /// </summary>
    public interface ITaskRepository
    {
        /// <summary>
        /// Gets all tasks
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of all tasks</returns>
        Task<IEnumerable<Domain.Entities.Task>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a task by its ID
        /// </summary>
        /// <param name="id">Task ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task with the specified ID, or null if not found</returns>
        Task<Domain.Entities.Task> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new task
        /// </summary>
        /// <param name="task">Task to add</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task AddAsync(Domain.Entities.Task task, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing task
        /// </summary>
        /// <param name="task">Task to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UpdateAsync(Domain.Entities.Task task, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a task
        /// </summary>
        /// <param name="id">ID of the task to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}