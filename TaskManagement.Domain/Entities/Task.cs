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
        /// <exception cref="ArgumentNullException">Thrown when title is null</exception>
        /// <exception cref="ArgumentException">Thrown when title is empty or whitespace</exception>
        public Task(string title, string description)
        {
            if (title == null)
                throw new ArgumentNullException(nameof(title), "Task title cannot be null");

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Task title cannot be empty or whitespace", nameof(title));

            Id = Guid.NewGuid();
            Title = title;
            Description = description ?? string.Empty;
            Status = TaskStatus.New;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the title of the task
        /// </summary>
        /// <param name="title">New title</param>
        /// <exception cref="ArgumentNullException">Thrown when title is null</exception>
        /// <exception cref="ArgumentException">Thrown when title is empty or whitespace</exception>
        public void UpdateTitle(string title)
        {
            if (title == null)
                throw new ArgumentNullException(nameof(title), "Task title cannot be null");

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Task title cannot be empty or whitespace", nameof(title));

            Title = title;
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
        /// Starts the task by setting its status to InProgress
        /// </summary>
        public void Start()
        {
            Status = TaskStatus.InProgress;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Completes the task by setting its status to Completed
        /// </summary>
        public void Complete()
        {
            Status = TaskStatus.Completed;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the status of the task
        /// </summary>
        /// <param name="status">New status</param>
        /// <exception cref="ArgumentException">Thrown when status is not a valid TaskStatus value</exception>
        public void UpdateStatus(TaskStatus status)
        {
            // Validate that the status is a defined enum value
            if (!Enum.IsDefined(typeof(TaskStatus), status))
            {
                throw new ArgumentException($"Invalid task status value: {status}", nameof(status));
            }

            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the status of the task using a string representation
        /// </summary>
        /// <param name="statusString">String representation of the status</param>
        /// <exception cref="ArgumentException">Thrown when statusString cannot be parsed to a valid TaskStatus</exception>
        public void UpdateStatus(string statusString)
        {
            if (string.IsNullOrWhiteSpace(statusString))
            {
                throw new ArgumentException("Status cannot be empty", nameof(statusString));
            }

            if (!Enum.TryParse<TaskStatus>(statusString, true, out var status))
            {
                throw new ArgumentException($"Invalid status value: {statusString}. Valid values are: {string.Join(", ", Enum.GetNames(typeof(TaskStatus)))}", nameof(statusString));
            }

            UpdateStatus(status);
        }
    }
}