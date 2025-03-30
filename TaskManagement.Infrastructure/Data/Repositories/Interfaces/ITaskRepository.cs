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
        /// <returns>List of all tasks</returns>
        Task<IEnumerable<Domain.Entities.Task>> GetAllAsync();

        /// <summary>
        /// Gets a task by its ID
        /// </summary>
        /// <param name="id">Task ID</param>
        /// <returns>Task with the specified ID, or null if not found</returns>
        Task<Domain.Entities.Task> GetByIdAsync(Guid id);

        /// <summary>
        /// Adds a new task
        /// </summary>
        /// <param name="task">Task to add</param>
        Task AddAsync(Domain.Entities.Task task);

        /// <summary>
        /// Updates an existing task
        /// </summary>
        /// <param name="task">Task to update</param>
        Task UpdateAsync(Domain.Entities.Task task);

        /// <summary>
        /// Deletes a task
        /// </summary>
        /// <param name="id">ID of the task to delete</param>
        Task DeleteAsync(Guid id);
    }
}
