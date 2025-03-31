using AutoMapper;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.DTOs.Mapping;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Events;
using TaskManagement.Infrastructure.Data.Repositories.Interfaces;
using TaskManagement.Infrastructure.Data.UnitOfWork;
using Task = System.Threading.Tasks.Task;

namespace TaskManagement.Tests.Application
{
    public class TaskServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITaskRepository> _mockTaskRepository;
        private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
        private readonly Mock<ILogger<TaskService>> _mockLogger;
        private readonly TaskService _taskService;
        private readonly IMapper _mapper;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public TaskServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTaskRepository = new Mock<ITaskRepository>();
            _mockPublishEndpoint = new Mock<IPublishEndpoint>();
            _mockLogger = new Mock<ILogger<TaskService>>();

            // Setup UnitOfWork to return the mocked repository
            _mockUnitOfWork.Setup(uow => uow.TaskRepository).Returns(_mockTaskRepository.Object);

            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<TaskMappingProfile>();
            }).CreateMapper();

            _taskService = new TaskService(
                _mockUnitOfWork.Object,
                _mockPublishEndpoint.Object,
                _mockLogger.Object,
                _mapper);
        }

        [Fact]
        public async Task GetAllTasksAsync_ShouldReturnAllTasks()
        {
            // Arrange
            var testTasks = new List<Domain.Entities.Task>
            {
                new Domain.Entities.Task("Test Task 1", "Description 1"),
                new Domain.Entities.Task("Test Task 2", "Description 2")
            };

            _mockTaskRepository.Setup(repo => repo.GetAllAsync(_cancellationToken))
                .ReturnsAsync(testTasks);

            // Act
            var result = await _taskService.GetAllTasksAsync(_cancellationToken);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, t => t.Title == "Test Task 1");
            Assert.Contains(result, t => t.Title == "Test Task 2");
        }

        [Fact]
        public async Task GetTaskByIdAsync_WithExistingId_ShouldReturnTask()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var testTask = new Domain.Entities.Task("Test Task", "Description");

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId, _cancellationToken))
                .ReturnsAsync(testTask);

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId, _cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Task", result.Title);
            Assert.Equal("Description", result.Description);
        }

        [Fact]
        public async Task GetTaskByIdAsync_WithEmptyId_ShouldThrowArgumentException()
        {
            // Arrange
            var taskId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _taskService.GetTaskByIdAsync(taskId, _cancellationToken));
        }

        [Fact]
        public async Task GetTaskByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId, _cancellationToken))
                .ReturnsAsync((Domain.Entities.Task)null);

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId, _cancellationToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateTaskAsync_ShouldCreateTaskAndPublishEvent()
        {
            // Arrange
            var createTaskDto = new CreateTaskDto
            {
                Title = "New Task",
                Description = "New Description"
            };

            Domain.Entities.Task savedTask = null;
            _mockTaskRepository.Setup(repo => repo.AddAsync(It.IsAny<Domain.Entities.Task>(), _cancellationToken))
                .Callback<Domain.Entities.Task, CancellationToken>((task, ct) => savedTask = task)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _taskService.CreateTaskAsync(createTaskDto, _cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createTaskDto.Title, result.Title);
            Assert.Equal(createTaskDto.Description, result.Description);

            _mockTaskRepository.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Task>(), _cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(_cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(_cancellationToken), Times.Once);

            _mockPublishEndpoint.Verify(pub => pub.Publish(
                It.Is<TaskCreatedEvent>(e =>
                    e.Title == createTaskDto.Title &&
                    e.Description == createTaskDto.Description),
                _cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task CreateTaskAsync_WithNullDto_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _taskService.CreateTaskAsync(null, _cancellationToken));
        }

        [Fact]
        public async Task CreateTaskAsync_WithEmptyTitle_ShouldThrowValidationException()
        {
            // Arrange
            var createTaskDto = new CreateTaskDto
            {
                Title = "",
                Description = "Description"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _taskService.CreateTaskAsync(createTaskDto, _cancellationToken));
        }

        [Fact]
        public async Task CreateTaskAsync_WithRepositoryError_ShouldRollbackAndThrow()
        {
            // Arrange
            var createTaskDto = new CreateTaskDto
            {
                Title = "New Task",
                Description = "New Description"
            };

            _mockTaskRepository.Setup(repo => repo.AddAsync(It.IsAny<Domain.Entities.Task>(), _cancellationToken))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<ServiceException>(() => _taskService.CreateTaskAsync(createTaskDto, _cancellationToken));

            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(_cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(_cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(_cancellationToken), Times.Never);
        }

        [Fact]
        public async Task UpdateTaskAsync_WithExistingId_ShouldUpdateTaskAndPublishEvent()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var updateTaskDto = new UpdateTaskDto
            {
                Title = "Updated Task",
                Description = "Updated Description",
                Status = "Completed"
            };

            var existingTask = new Domain.Entities.Task("Original Task", "Original Description");

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId, _cancellationToken))
                .ReturnsAsync(existingTask);

            _mockTaskRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Task>(), _cancellationToken))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _taskService.UpdateTaskAsync(taskId, updateTaskDto, _cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateTaskDto.Title, result.Title);
            Assert.Equal(updateTaskDto.Description, result.Description);
            Assert.Equal(updateTaskDto.Status, result.Status);

            _mockTaskRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Task>(), _cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(_cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(_cancellationToken), Times.Once);

            _mockPublishEndpoint.Verify(pub => pub.Publish(
                It.IsAny<TaskUpdatedEvent>(),
                _cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task UpdateTaskAsync_WithEmptyId_ShouldThrowArgumentException()
        {
            // Arrange
            var updateTaskDto = new UpdateTaskDto
            {
                Title = "Updated Task"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _taskService.UpdateTaskAsync(Guid.Empty, updateTaskDto, _cancellationToken));
        }

        [Fact]
        public async Task UpdateTaskAsync_WithNullDto_ShouldThrowArgumentNullException()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _taskService.UpdateTaskAsync(taskId, null, _cancellationToken));
        }

        [Fact]
        public async Task UpdateTaskAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var updateTaskDto = new UpdateTaskDto
            {
                Title = "Updated Task",
                Description = "Updated Description",
                Status = "Completed"
            };

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId, _cancellationToken))
                .ReturnsAsync((Domain.Entities.Task)null);

            // Act
            var result = await _taskService.UpdateTaskAsync(taskId, updateTaskDto, _cancellationToken);

            // Assert
            Assert.Null(result);

            _mockTaskRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Task>(), _cancellationToken), Times.Never);
            _mockPublishEndpoint.Verify(pub => pub.Publish(It.IsAny<TaskUpdatedEvent>(), _cancellationToken), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(_cancellationToken), Times.Never);
        }

        [Fact]
        public async Task UpdateTaskAsync_WithRepositoryError_ShouldRollbackAndThrow()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var updateTaskDto = new UpdateTaskDto
            {
                Title = "Updated Task",
                Description = "Updated Description",
                Status = "Completed"
            };

            var existingTask = new Domain.Entities.Task("Original Task", "Original Description");

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId, _cancellationToken))
                .ReturnsAsync(existingTask);

            _mockTaskRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Task>(), _cancellationToken))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<ServiceException>(() => _taskService.UpdateTaskAsync(taskId, updateTaskDto, _cancellationToken));

            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(_cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(_cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(_cancellationToken), Times.Never);
        }

        [Fact]
        public async Task DeleteTaskAsync_WithExistingId_ShouldDeleteTaskAndPublishEvent()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var existingTask = new Domain.Entities.Task("Test Task", "Description");

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId, _cancellationToken))
                .ReturnsAsync(existingTask);

            _mockTaskRepository.Setup(repo => repo.DeleteAsync(taskId, _cancellationToken))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _taskService.DeleteTaskAsync(taskId, _cancellationToken);

            // Assert
            Assert.True(result);

            _mockTaskRepository.Verify(repo => repo.DeleteAsync(taskId, _cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(_cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(_cancellationToken), Times.Once);

            _mockPublishEndpoint.Verify(pub => pub.Publish(
                It.Is<TaskDeletedEvent>(e => e.TaskId == taskId),
                _cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task DeleteTaskAsync_WithEmptyId_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _taskService.DeleteTaskAsync(Guid.Empty, _cancellationToken));
        }

        [Fact]
        public async Task DeleteTaskAsync_WithNonExistingId_ShouldReturnFalse()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId, _cancellationToken))
                .ReturnsAsync((Domain.Entities.Task)null);

            // Act
            var result = await _taskService.DeleteTaskAsync(taskId, _cancellationToken);

            // Assert
            Assert.False(result);

            _mockTaskRepository.Verify(repo => repo.DeleteAsync(taskId, _cancellationToken), Times.Never);
            _mockPublishEndpoint.Verify(pub => pub.Publish(It.IsAny<TaskDeletedEvent>(), _cancellationToken), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(_cancellationToken), Times.Never);
        }

        [Fact]
        public async Task DeleteTaskAsync_WithRepositoryError_ShouldRollbackAndThrow()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var existingTask = new Domain.Entities.Task("Test Task", "Description");

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId, _cancellationToken))
                .ReturnsAsync(existingTask);

            _mockTaskRepository.Setup(repo => repo.DeleteAsync(taskId, _cancellationToken))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<ServiceException>(() => _taskService.DeleteTaskAsync(taskId, _cancellationToken));

            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(_cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(_cancellationToken), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(_cancellationToken), Times.Never);
        }

        [Fact]
        public async Task Operations_WithCancelledToken_ShouldPropagateCancellation()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            _mockTaskRepository.Setup(repo => repo.GetAllAsync(cts.Token))
                .ThrowsAsync(new OperationCanceledException());

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId, cts.Token))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                _taskService.GetAllTasksAsync(cts.Token));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                _taskService.GetTaskByIdAsync(taskId, cts.Token));
        }
    }
}