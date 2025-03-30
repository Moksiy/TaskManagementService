using MassTransit;
using Microsoft.Extensions.Logging;
using TaskManagement.Domain.Events;

namespace TaskManagement.MessageConsumer.Consumers
{
    /// <summary>
    /// Consumer for task created events
    /// </summary>
    public class TaskCreatedConsumer : IConsumer<TaskCreatedEvent>
    {
        private readonly ILogger<TaskCreatedConsumer> _logger;

        /// <summary>
        /// Creates a new TaskCreatedConsumer
        /// </summary>
        /// <param name="logger">Logger</param>
        public TaskCreatedConsumer(ILogger<TaskCreatedConsumer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Consumes a task created event
        /// </summary>
        /// <param name="context">Consume context</param>
        public Task Consume(ConsumeContext<TaskCreatedEvent> context)
        {
            var eventData = context.Message;

            _logger.LogInformation("Task created event received: {TaskId} - {Title}",
                eventData.TaskId,
                eventData.Title);

            return Task.CompletedTask;
        }
    }
}
