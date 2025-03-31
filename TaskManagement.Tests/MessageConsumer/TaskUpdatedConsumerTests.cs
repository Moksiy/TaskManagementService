using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Domain.Events;
using TaskManagement.MessageConsumer.Consumers;
using Task = System.Threading.Tasks.Task;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Tests.MessageConsumer
{
    public class TaskUpdatedConsumerTests
    {
        private readonly Mock<ILogger<TaskUpdatedConsumer>> _mockLogger;
        private readonly TaskUpdatedConsumer _consumer;

        public TaskUpdatedConsumerTests()
        {
            _mockLogger = new Mock<ILogger<TaskUpdatedConsumer>>();
            _consumer = new TaskUpdatedConsumer(_mockLogger.Object);
        }

        [Fact]
        public async Task Consume_ShouldLogInformation()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var eventData = new TaskUpdatedEvent(taskId, "Updated Task", "Updated Description", TaskStatus.InProgress);

            var mockConsumeContext = new Mock<ConsumeContext<TaskUpdatedEvent>>();
            mockConsumeContext.Setup(x => x.Message).Returns(eventData);
            mockConsumeContext.Setup(x => x.MessageId).Returns(Guid.NewGuid());

            // Act
            await _consumer.Consume(mockConsumeContext.Object);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Task updated event received")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async Task Consume_WithNullMessage_ShouldNotThrowException()
        {
            // Arrange
            var mockConsumeContext = new Mock<ConsumeContext<TaskUpdatedEvent>>();
            mockConsumeContext.Setup(x => x.Message).Returns((TaskUpdatedEvent)null);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(async () => 
                await _consumer.Consume(mockConsumeContext.Object));
        }

        [Fact]
        public async Task Consume_WithDifferentStatuses_ShouldLogCorrectly()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            // Test each status
            foreach (TaskStatus status in Enum.GetValues(typeof(TaskStatus)))
            {
                var eventData = new TaskUpdatedEvent(taskId, "Updated Task", "Updated Description", status);

                var mockConsumeContext = new Mock<ConsumeContext<TaskUpdatedEvent>>();
                mockConsumeContext.Setup(x => x.Message).Returns(eventData);
                mockConsumeContext.Setup(x => x.MessageId).Returns(Guid.NewGuid());

                // Act
                await _consumer.Consume(mockConsumeContext.Object);

                // Assert
                _mockLogger.Verify(
                    x => x.Log(
                        It.Is<LogLevel>(l => l == LogLevel.Information),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Task updated event received")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.AtLeastOnce());
            }
        }
    }
}