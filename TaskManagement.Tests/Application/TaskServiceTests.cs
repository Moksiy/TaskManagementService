using AutoMapper;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.DTOs.Mapping;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Events;
using TaskManagement.Infrastructure.Data.Repositories.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace TaskManagement.Tests.Application
{
    public class TaskServiceTests
    {
        private readonly Mock<ITaskRepository> _mockTaskRepository;
        private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
        private readonly Mock<ILogger<TaskService>> _mockLogger;
        private readonly TaskService _taskService;
        private readonly IMapper _mapper;

        public TaskServiceTests()
        {
            _mockTaskRepository = new Mock<ITaskRepository>();
            _mockPublishEndpoint = new Mock<IPublishEndpoint>();
            _mockLogger = new Mock<ILogger<TaskService>>();
            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<TaskMappingProfile>();
            }).CreateMapper();

            _taskService = new TaskService(
                _mockTaskRepository.Object,
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

            _mockTaskRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(testTasks);

            // Act
            var result = await _taskService.GetAllTasksAsync();

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

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
                .ReturnsAsync(testTask);

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Task", result.Title);
            Assert.Equal("Description", result.Description);
        }

        [Fact]
        public async Task GetTaskByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
                .ReturnsAsync((Domain.Entities.Task)null);

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId);

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
            _mockTaskRepository.Setup(repo => repo.AddAsync(It.IsAny<Domain.Entities.Task>()))
                .Callback<Domain.Entities.Task>(task => savedTask = task)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _taskService.CreateTaskAsync(createTaskDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createTaskDto.Title, result.Title);
            Assert.Equal(createTaskDto.Description, result.Description);

            _mockTaskRepository.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Task>()), Times.Once);

            _mockPublishEndpoint.Verify(pub => pub.Publish(
                It.Is<TaskCreatedEvent>(e =>
                    e.Title == createTaskDto.Title &&
                    e.Description == createTaskDto.Description),
                default),
                Times.Once);
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

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
                .ReturnsAsync(existingTask);

            _mockTaskRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Task>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _taskService.UpdateTaskAsync(taskId, updateTaskDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateTaskDto.Title, result.Title);
            Assert.Equal(updateTaskDto.Description, result.Description);
            Assert.Equal(updateTaskDto.Status, result.Status);

            _mockTaskRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Task>()), Times.Once);

            _mockPublishEndpoint.Verify(pub => pub.Publish(
                It.IsAny<TaskUpdatedEvent>(),
                default),
                Times.Once);
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

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
                .ReturnsAsync((Domain.Entities.Task)null);

            // Act
            var result = await _taskService.UpdateTaskAsync(taskId, updateTaskDto);

            // Assert
            Assert.Null(result);

            _mockTaskRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Task>()), Times.Never);
            _mockPublishEndpoint.Verify(pub => pub.Publish(It.IsAny<TaskUpdatedEvent>(), default), Times.Never);
        }

        [Fact]
        public async Task DeleteTaskAsync_WithExistingId_ShouldDeleteTaskAndPublishEvent()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var existingTask = new Domain.Entities.Task("Test Task", "Description");

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
                .ReturnsAsync(existingTask);

            _mockTaskRepository.Setup(repo => repo.DeleteAsync(taskId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _taskService.DeleteTaskAsync(taskId);

            // Assert
            Assert.True(result);

            _mockTaskRepository.Verify(repo => repo.DeleteAsync(taskId), Times.Once);

            _mockPublishEndpoint.Verify(pub => pub.Publish(
                It.Is<TaskDeletedEvent>(e => e.TaskId == taskId),
                default),
                Times.Once);
        }

        [Fact]
        public async Task DeleteTaskAsync_WithNonExistingId_ShouldReturnFalse()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
                .ReturnsAsync((Domain.Entities.Task)null);

            // Act
            var result = await _taskService.DeleteTaskAsync(taskId);

            // Assert
            Assert.False(result);

            _mockTaskRepository.Verify(repo => repo.DeleteAsync(taskId), Times.Never);
            _mockPublishEndpoint.Verify(pub => pub.Publish(It.IsAny<TaskDeletedEvent>(), default), Times.Never);
        }
    }
}