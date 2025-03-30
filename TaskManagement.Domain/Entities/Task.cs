using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Domain.Entities
{
    /// <summary>
    /// User task
    /// </summary>
    public class Task
    {
        /// <summary>
        /// Unique identifier of the task
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Title of the task
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Description of the task
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Current status of the task
        /// </summary>
        public TaskStatus Status { get; private set; }

        /// <summary>
        /// Date and time when the task was created
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Date and time when the task was last updated
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        // Private constructor for EF Core
        private Task() { }

        /// <summary>
        /// Creates a new task with the specified title and description
        /// </summary>
        /// <param name="title">Title of the task</param>
        /// <param name="description">Description of the task</param>
        public Task(string title, string description)
        {
            Id = Guid.NewGuid();
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Description = description ?? string.Empty;
            Status = TaskStatus.New;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the title of the task
        /// </summary>
        /// <param name="title">New title</param>
        public void UpdateTitle(string title)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the description of the task
        /// </summary>
        /// <param name="description">New description</param>
        public void UpdateDescription(string description)
        {
            Description = description ?? string.Empty;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the status of the task
        /// </summary>
        /// <param name="status">New status</param>
        public void UpdateStatus(TaskStatus status)
        {
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
