using MassTransit;
using Microsoft.Extensions.Logging;
using TaskManagement.Domain.Events;

namespace TaskManagement.MessageConsumer.Consumers
{
    /// <summary>
    /// Consumer for task updated events
    /// </summary>
    public class TaskUpdatedConsumer : IConsumer<TaskUpdatedEvent>
    {
        private readonly ILogger<TaskUpdatedConsumer> _logger;

        /// <summary>
        /// Creates a new TaskUpdatedConsumer
        /// </summary>
        /// <param name="logger">Logger</param>
        public TaskUpdatedConsumer(ILogger<TaskUpdatedConsumer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Consumes a task updated event
        /// </summary>
        /// <param name="context">Consume context</param>
        public Task Consume(ConsumeContext<TaskUpdatedEvent> context)
        {
            var eventData = context.Message;

            _logger.LogInformation("Task updated event received: {TaskId} - {Title} - {Status}",
                eventData.TaskId,
                eventData.Title,
                eventData.Status);

            return Task.CompletedTask;
        }
    }
}
