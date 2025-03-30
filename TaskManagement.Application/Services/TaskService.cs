using MassTransit;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Services.Interfaces;
using TaskManagement.Domain;
using TaskManagement.Domain.Events;
using TaskManagement.Infrastructure.Data.Repositories.Interfaces;

namespace TaskManagement.Application.Services
{
    /// <summary>
    /// Implementation of task management service
    /// </summary>
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<TaskService> _logger;

        /// <summary>
        /// Creates a new TaskService
        /// </summary>
        /// <param name="taskRepository">Task repository</param>
        /// <param name="publishEndpoint">Message broker publish endpoint</param>
        /// <param name="logger">Logger</param>
        public TaskService(
            ITaskRepository taskRepository,
            IPublishEndpoint publishEndpoint,
            ILogger<TaskService> logger)
        {
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
        {
            _logger.LogInformation("Getting all tasks");
            var tasks = await _taskRepository.GetAllAsync();
            return tasks.Select(MapToDto);
        }

        /// <inheritdoc />
        public async Task<TaskDto> GetTaskByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting task with ID: {TaskId}", id);
            var task = await _taskRepository.GetByIdAsync(id);
            return task != null ? MapToDto(task) : null;
        }

        /// <inheritdoc />
        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto)
        {
            _logger.LogInformation("Creating new task with title: {TaskTitle}", createTaskDto.Title);

            var task = new Domain.Entities.Task(createTaskDto.Title, createTaskDto.Description);
            await _taskRepository.AddAsync(task);

            // Publish event to message broker
            await _publishEndpoint.Publish(new TaskCreatedEvent(
                task.Id,
                task.Title,
                task.Description
            ));

            return MapToDto(task);
        }

        /// <inheritdoc />
        public async Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskDto updateTaskDto)
        {
            _logger.LogInformation("Updating task with ID: {TaskId}", id);

            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for update", id);
                return null;
            }

            // Update task properties if provided
            if (!string.IsNullOrWhiteSpace(updateTaskDto.Title))
            {
                task.UpdateTitle(updateTaskDto.Title);
            }

            if (updateTaskDto.Description != null)
            {
                task.UpdateDescription(updateTaskDto.Description);
            }

            if (!string.IsNullOrWhiteSpace(updateTaskDto.Status) &&
                Enum.TryParse<Domain.Enums.TaskStatus>(updateTaskDto.Status, true, out var status))
            {
                task.UpdateStatus(status);
            }

            await _taskRepository.UpdateAsync(task);

            // Publish event to message broker
            await _publishEndpoint.Publish(new TaskUpdatedEvent(
                task.Id,
                task.Title,
                task.Description,
                task.Status
            ));

            return MapToDto(task);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteTaskAsync(Guid id)
        {
            _logger.LogInformation("Deleting task with ID: {TaskId}", id);

            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for deletion", id);
                return false;
            }

            await _taskRepository.DeleteAsync(id);

            // Publish event to message broker
            await _publishEndpoint.Publish(new TaskDeletedEvent(id));

            return true;
        }

        private static TaskDto MapToDto(Domain.Entities.Task task)
        {
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }
    }
}
