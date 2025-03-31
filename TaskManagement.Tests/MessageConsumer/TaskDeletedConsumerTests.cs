using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Domain.Events;
using TaskManagement.MessageConsumer.Consumers;
using Xunit;

namespace TaskManagement.Tests.MessageConsumer
{
    public class TaskDeletedConsumerTests
    {
        private readonly Mock<ILogger<TaskDeletedConsumer>> _mockLogger;
        private readonly TaskDeletedConsumer _consumer;

        public TaskDeletedConsumerTests()
        {
            _mockLogger = new Mock<ILogger<TaskDeletedConsumer>>();
            _consumer = new TaskDeletedConsumer(_mockLogger.Object);
        }

        [Fact]
        public async Task Consume_ShouldLogInformation()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var eventData = new TaskDeletedEvent(taskId);

            var mockConsumeContext = new Mock<ConsumeContext<TaskDeletedEvent>>();
            mockConsumeContext.Setup(x => x.Message).Returns(eventData);
            mockConsumeContext.Setup(x => x.MessageId).Returns(Guid.NewGuid());
            mockConsumeContext.Setup(x => x.CorrelationId).Returns(Guid.NewGuid());

            // Act
            await _consumer.Consume(mockConsumeContext.Object);

            // Assert
            // Verify that logging occurred (at least at Information level)
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Task deleted event received")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async Task Consume_ShouldLogSuccessfulProcessing()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var eventData = new TaskDeletedEvent(taskId);

            var mockConsumeContext = new Mock<ConsumeContext<TaskDeletedEvent>>();
            mockConsumeContext.Setup(x => x.Message).Returns(eventData);
            mockConsumeContext.Setup(x => x.MessageId).Returns(Guid.NewGuid());

            // Act
            await _consumer.Consume(mockConsumeContext.Object);

            // Assert
            // Verify that successful processing was logged
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Task deleted event received")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once());
        }

        [Fact]
        public async Task Consume_WithNullMessage_ShouldNotThrowException()
        {
            // Arrange
            var mockConsumeContext = new Mock<ConsumeContext<TaskDeletedEvent>>();
            mockConsumeContext.Setup(x => x.Message).Returns((TaskDeletedEvent)null);

            // Act & Assert
            // Should not throw exception
            await Assert.ThrowsAsync<NullReferenceException>(async () => 
                await _consumer.Consume(mockConsumeContext.Object));
        }
    }
}