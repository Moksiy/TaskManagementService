using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Services.Interfaces
{
    /// <summary>
    /// Interface for task management service
    /// </summary>
    public interface ITaskService
    {
        /// <summary>
        /// Gets all tasks
        /// </summary>
        /// <returns>List of all tasks</returns>
        Task<IEnumerable<TaskDto>> GetAllTasksAsync();

        /// <summary>
        /// Gets a task by its ID
        /// </summary>
        /// <param name="id">Task ID</param>
        /// <returns>Task with the specified ID, or null if not found</returns>
        Task<TaskDto> GetTaskByIdAsync(Guid id);

        /// <summary>
        /// Creates a new task
        /// </summary>
        /// <param name="createTaskDto">Task data</param>
        /// <returns>Created task</returns>
        Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto);

        /// <summary>
        /// Updates an existing task
        /// </summary>
        /// <param name="id">Task ID</param>
        /// <param name="updateTaskDto">Updated task data</param>
        /// <returns>Updated task, or null if task not found</returns>
        Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskDto updateTaskDto);

        /// <summary>
        /// Deletes a task
        /// </summary>
        /// <param name="id">Task ID</param>
        /// <returns>True if task was deleted, false if task was not found</returns>
        Task<bool> DeleteTaskAsync(Guid id);
    }
}
