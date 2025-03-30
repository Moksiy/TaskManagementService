using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for creating a new task
    /// </summary>
    public class CreateTaskDto
    {
        /// <summary>
        /// Title of the task
        /// </summary>
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; }

        /// <summary>
        /// Description of the task
        /// </summary>
        [StringLength(1000)]
        public string Description { get; set; }
    }
}
