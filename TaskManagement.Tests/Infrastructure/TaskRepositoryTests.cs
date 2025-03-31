using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Infrastructure.Data.Context;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Tests.Infrastructure
{
    public class TaskRepositoryTests
    {
        private DbContextOptions<TaskManagementDbContext> CreateNewContextOptions()
        {
            // Create a fresh in-memory database for each test
            return new DbContextOptionsBuilder<TaskManagementDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllTasks()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var mockLogger = new Mock<ILogger<TaskRepository>>();

            // Seed the database
            using (var context = new TaskManagementDbContext(options))
            {
                context.Tasks.Add(new Domain.Entities.Task("Task 1", "Description 1"));
                context.Tasks.Add(new Domain.Entities.Task("Task 2", "Description 2"));
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new TaskManagementDbContext(options))
            {
                var repository = new TaskRepository(context, mockLogger.Object);
                var tasks = await repository.GetAllAsync(CancellationToken.None);

                // Assert
                Assert.Equal(2, tasks.Count());
                Assert.Contains(tasks, t => t.Title == "Task 1");
                Assert.Contains(tasks, t => t.Title == "Task 2");
            }
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ShouldReturnTask()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var mockLogger = new Mock<ILogger<TaskRepository>>();
            var taskId = Guid.NewGuid();
            var task = new Domain.Entities.Task("Task 1", "Description 1");

            var idProperty = task.GetType().GetProperty("Id");
            idProperty.SetValue(task, taskId);

            using (var context = new TaskManagementDbContext(options))
            {
                context.Tasks.Add(task);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new TaskManagementDbContext(options))
            {
                var repository = new TaskRepository(context, mockLogger.Object);
                var result = await repository.GetByIdAsync(taskId, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("Task 1", result.Title);
                Assert.Equal("Description 1", result.Description);
            }
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var mockLogger = new Mock<ILogger<TaskRepository>>();
            var nonExistingId = Guid.NewGuid();

            // Act
            using (var context = new TaskManagementDbContext(options))
            {
                var repository = new TaskRepository(context, mockLogger.Object);
                var result = await repository.GetByIdAsync(nonExistingId, CancellationToken.None);

                // Assert
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task AddAsync_ShouldAddTaskToDatabase()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var mockLogger = new Mock<ILogger<TaskRepository>>();
            var task = new Domain.Entities.Task("New Task", "New Description");

            // Act
            using (var context = new TaskManagementDbContext(options))
            {
                var repository = new TaskRepository(context, mockLogger.Object);
                await repository.AddAsync(task, CancellationToken.None);
            }

            // Assert
            using (var context = new TaskManagementDbContext(options))
            {
                Assert.Equal(1, await context.Tasks.CountAsync());
                var savedTask = await context.Tasks.FirstOrDefaultAsync();
                Assert.NotNull(savedTask);
                Assert.Equal("New Task", savedTask.Title);
                Assert.Equal("New Description", savedTask.Description);
            }
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingTask()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var mockLogger = new Mock<ILogger<TaskRepository>>();
            var task = new Domain.Entities.Task("Original Task", "Original Description");

            using (var context = new TaskManagementDbContext(options))
            {
                context.Tasks.Add(task);
                await context.SaveChangesAsync();
            }

            // Update task properties
            task.UpdateTitle("Updated Task");
            task.UpdateDescription("Updated Description");
            task.UpdateStatus(TaskStatus.Completed);

            // Act
            using (var context = new TaskManagementDbContext(options))
            {
                var repository = new TaskRepository(context, mockLogger.Object);
                await repository.UpdateAsync(task, CancellationToken.None);
            }

            // Assert
            using (var context = new TaskManagementDbContext(options))
            {
                var updatedTask = await context.Tasks.FirstOrDefaultAsync();
                Assert.NotNull(updatedTask);
                Assert.Equal("Updated Task", updatedTask.Title);
                Assert.Equal("Updated Description", updatedTask.Description);
                Assert.Equal(TaskStatus.Completed, updatedTask.Status);
            }
        }

        [Fact]
        public async Task DeleteAsync_WithExistingId_ShouldRemoveTaskFromDatabase()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var mockLogger = new Mock<ILogger<TaskRepository>>();
            var taskId = Guid.NewGuid();
            var task = new Domain.Entities.Task("Task to Delete", "Description");

            var idProperty = task.GetType().GetProperty("Id");
            idProperty.SetValue(task, taskId);

            using (var context = new TaskManagementDbContext(options))
            {
                context.Tasks.Add(task);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new TaskManagementDbContext(options))
            {
                var repository = new TaskRepository(context, mockLogger.Object);
                await repository.DeleteAsync(taskId, CancellationToken.None);
            }

            // Assert
            using (var context = new TaskManagementDbContext(options))
            {
                Assert.Equal(0, await context.Tasks.CountAsync());
            }
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ShouldNotThrowException()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var mockLogger = new Mock<ILogger<TaskRepository>>();
            var nonExistingId = Guid.NewGuid();

            // Act & Assert
            using (var context = new TaskManagementDbContext(options))
            {
                var repository = new TaskRepository(context, mockLogger.Object);

                // Should not throw exception
                await repository.DeleteAsync(nonExistingId, CancellationToken.None);
            }
        }

        [Fact]
        public async Task GetAllAsync_WithCancelledToken_ShouldThrowCancellationException()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var mockLogger = new Mock<ILogger<TaskRepository>>();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel(); // Cancel the token immediately

            // Act & Assert
            using (var context = new TaskManagementDbContext(options))
            {
                var repository = new TaskRepository(context, mockLogger.Object);

                await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                    await repository.GetAllAsync(cancellationTokenSource.Token));
            }
        }

        [Fact]
        public async Task GetByIdAsync_WithCancelledToken_ShouldThrowCancellationException()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var mockLogger = new Mock<ILogger<TaskRepository>>();
            var taskId = Guid.NewGuid();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel(); // Cancel the token immediately

            // Act & Assert
            using (var context = new TaskManagementDbContext(options))
            {
                var repository = new TaskRepository(context, mockLogger.Object);

                await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                    await repository.GetByIdAsync(taskId, cancellationTokenSource.Token));
            }
        }

        [Fact]
        public async Task AddAsync_WithCancelledToken_ShouldThrowCancellationException()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var mockLogger = new Mock<ILogger<TaskRepository>>();
            var task = new Domain.Entities.Task("New Task", "New Description");
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel(); // Cancel the token immediately

            // Act & Assert
            using (var context = new TaskManagementDbContext(options))
            {
                var repository = new TaskRepository(context, mockLogger.Object);

                await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                    await repository.AddAsync(task, cancellationTokenSource.Token));
            }
        }
    }
}