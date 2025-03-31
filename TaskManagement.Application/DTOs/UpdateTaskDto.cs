using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for updating an existing task
    /// </summary>
    public class UpdateTaskDto
    {
        /// <summary>
        /// Title of the task
        /// </summary>
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
        public string Title { get; set; }

        /// <summary>
        /// Description of the task
        /// </summary>
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }

        /// <summary>
        /// Current status of the task (New, InProgress, Completed)
        /// </summary>
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
    }
}