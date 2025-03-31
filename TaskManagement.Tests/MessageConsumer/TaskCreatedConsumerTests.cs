using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Domain.Events;
using TaskManagement.MessageConsumer.Consumers;

namespace TaskManagement.Tests.MessageConsumer
{
    public class TaskCreatedConsumerTests
    {
        private readonly Mock<ILogger<TaskCreatedConsumer>> _mockLogger;
        private readonly TaskCreatedConsumer _consumer;

        public TaskCreatedConsumerTests()
        {
            _mockLogger = new Mock<ILogger<TaskCreatedConsumer>>();
            _consumer = new TaskCreatedConsumer(_mockLogger.Object);
        }

        [Fact]
        public async Task Consume_ShouldLogInformation()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var eventData = new TaskCreatedEvent(taskId, "Test Task", "Test Description");

            var mockConsumeContext = new Mock<ConsumeContext<TaskCreatedEvent>>();
            mockConsumeContext.Setup(x => x.Message).Returns(eventData);
            mockConsumeContext.Setup(x => x.MessageId).Returns(Guid.NewGuid());

            // Act
            await _consumer.Consume(mockConsumeContext.Object);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Task created event received")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async Task Consume_WithNullMessage_ShouldNotThrowException()
        {
            // Arrange
            var mockConsumeContext = new Mock<ConsumeContext<TaskCreatedEvent>>();
            mockConsumeContext.Setup(x => x.Message).Returns((TaskCreatedEvent)null);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
                await _consumer.Consume(mockConsumeContext.Object));
        }

        [Fact]
        public async Task Consume_WithLongDescription_ShouldTruncateInLog()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var longDescription = new string('A', 100);
            var eventData = new TaskCreatedEvent(taskId, "Test Task", longDescription);

            var mockConsumeContext = new Mock<ConsumeContext<TaskCreatedEvent>>();
            mockConsumeContext.Setup(x => x.Message).Returns(eventData);
            mockConsumeContext.Setup(x => x.MessageId).Returns(Guid.NewGuid());

            // Act
            await _consumer.Consume(mockConsumeContext.Object);

            // Assert 
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Task created event received")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce());
        }
    }
}