namespace TaskManagement.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for Task entity
    /// </summary>
    public class TaskDto
    {
        /// <summary>
        /// Unique identifier of the task
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Title of the task
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description of the task
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Current status of the task (New, InProgress, Completed)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Date and time when the task was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date and time when the task was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
