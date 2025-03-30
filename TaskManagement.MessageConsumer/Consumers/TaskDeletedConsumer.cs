using MassTransit;
using Microsoft.Extensions.Logging;
using TaskManagement.Domain.Events;

namespace TaskManagement.MessageConsumer.Consumers
{
    /// <summary>
    /// Consumer for task deleted events
    /// </summary>
    public class TaskDeletedConsumer : IConsumer<TaskDeletedEvent>
    {
        private readonly ILogger<TaskDeletedConsumer> _logger;

        /// <summary>
        /// Creates a new TaskDeletedConsumer
        /// </summary>
        /// <param name="logger">Logger</param>
        public TaskDeletedConsumer(ILogger<TaskDeletedConsumer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Consumes a task deleted event
        /// </summary>
        /// <param name="context">Consume context</param>
        public Task Consume(ConsumeContext<TaskDeletedEvent> context)
        {
            var eventData = context.Message;

            _logger.LogInformation("Task deleted event received: {TaskId}",eventData.TaskId);

            return Task.CompletedTask;
        }
    }
}
