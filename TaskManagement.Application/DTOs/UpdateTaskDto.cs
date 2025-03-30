using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs
{
    // <summary>
    /// Data Transfer Object for updating an existing task
    /// </summary>
    public class UpdateTaskDto
    {
        /// <summary>
        /// Title of the task
        /// </summary>
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; }

        /// <summary>
        /// Description of the task
        /// </summary>
        [StringLength(1000)]
        public string Description { get; set; }

        /// <summary>
        /// Current status of the task (New, InProgress, Completed)
        /// </summary>
        public string Status { get; set; }
    }
}
