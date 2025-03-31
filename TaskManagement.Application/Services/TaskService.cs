using AutoMapper;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Services.Interfaces;
using TaskManagement.Domain.Events;
using TaskManagement.Infrastructure.Data.UnitOfWork;

namespace TaskManagement.Application.Services
{
    /// <summary>
    /// Implementation of task management service
    /// </summary>
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<TaskService> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Creates a new TaskService
        /// </summary>
        /// <param name="unitOfWork">Unit of work for transaction management</param>
        /// <param name="publishEndpoint">Message broker publish endpoint</param>
        /// <param name="logger">Logger</param>
        /// <param name="mapper">Object mapper</param>
        public TaskService(
            IUnitOfWork unitOfWork,
            IPublishEndpoint publishEndpoint,
            ILogger<TaskService> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TaskDto>> GetAllTasksAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all tasks");
            var tasks = await _unitOfWork.TaskRepository.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }

        /// <inheritdoc />
        public async Task<TaskDto?> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Task ID cannot be empty", nameof(id));
            }

            _logger.LogInformation("Getting task with ID: {TaskId}", id);
            var task = await _unitOfWork.TaskRepository.GetByIdAsync(id, cancellationToken);

            // Return null if task not found instead of throwing exception
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found", id);
                return null;
            }

            return _mapper.Map<TaskDto>(task);
        }

        /// <inheritdoc />
        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, CancellationToken cancellationToken = default)
        {
            if (createTaskDto == null)
            {
                throw new ArgumentNullException(nameof(createTaskDto), "Task creation data cannot be null");
            }

            // Validate title is not empty
            if (string.IsNullOrWhiteSpace(createTaskDto.Title))
            {
                throw new ValidationException("Task title cannot be empty");
            }

            _logger.LogInformation("Creating new task with title: {TaskTitle}", createTaskDto.Title);

            try
            {
                // Begin transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var task = new Domain.Entities.Task(createTaskDto.Title, createTaskDto.Description);
                await _unitOfWork.TaskRepository.AddAsync(task, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish event to message broker
                await _publishEndpoint.Publish(new TaskCreatedEvent(
                    task.Id,
                    task.Title,
                    task.Description
                ), cancellationToken);

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                return _mapper.Map<TaskDto>(task);
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error creating task: {Message}", ex.Message);
                throw new ServiceException("Failed to create task", ex);
            }
        }

        /// <inheritdoc />
        public async Task<TaskDto?> UpdateTaskAsync(Guid id, UpdateTaskDto updateTaskDto, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Task ID cannot be empty", nameof(id));
            }

            if (updateTaskDto == null)
            {
                throw new ArgumentNullException(nameof(updateTaskDto), "Task update data cannot be null");
            }

            _logger.LogInformation("Updating task with ID: {TaskId}", id);

            try
            {
                // Begin transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var task = await _unitOfWork.TaskRepository.GetByIdAsync(id, cancellationToken);
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

                // Handle null description differently - it can be explicitly set to null
                if (updateTaskDto.Description != null)
                {
                    task.UpdateDescription(updateTaskDto.Description);
                }

                if (!string.IsNullOrWhiteSpace(updateTaskDto.Status) &&
                    Enum.TryParse<Domain.Enums.TaskStatus>(updateTaskDto.Status, true, out var status))
                {
                    task.UpdateStatus(status);
                }

                await _unitOfWork.TaskRepository.UpdateAsync(task, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish event to message broker
                await _publishEndpoint.Publish(new TaskUpdatedEvent(
                    task.Id,
                    task.Title,
                    task.Description,
                    task.Status
                ), cancellationToken);

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                return _mapper.Map<TaskDto>(task);
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error updating task: {Message}", ex.Message);
                throw new ServiceException("Failed to update task", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Task ID cannot be empty", nameof(id));
            }

            _logger.LogInformation("Deleting task with ID: {TaskId}", id);

            try
            {
                // Begin transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var task = await _unitOfWork.TaskRepository.GetByIdAsync(id, cancellationToken);
                if (task == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found for deletion", id);
                    return false;
                }

                await _unitOfWork.TaskRepository.DeleteAsync(id, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish event to message broker
                await _publishEndpoint.Publish(new TaskDeletedEvent(id), cancellationToken);

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error deleting task: {Message}", ex.Message);
                throw new ServiceException("Failed to delete task", ex);
            }
        }
    }
}