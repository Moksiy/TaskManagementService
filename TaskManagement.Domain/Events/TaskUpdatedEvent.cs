using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Domain.Events
{
    /// <summary>
    /// Event raised when a task is updated
    /// </summary>
    public class TaskUpdatedEvent : TaskEvent
    {
        /// <summary>
        /// Updated title of the task
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Updated description of the task
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Updated status of the task
        /// </summary>
        public Enums.TaskStatus Status { get; }

        /// <summary>
        /// Creates a new TaskUpdatedEvent
        /// </summary>
        /// <param name="taskId">ID of the updated task</param>
        /// <param name="title">Updated title</param>
        /// <param name="description">Updated description</param>
        /// <param name="status">Updated status</param>
        public TaskUpdatedEvent(Guid taskId, string title, string description, Enums.TaskStatus status) : base(taskId)
        {
            Title = title;
            Description = description;
            Status = status;
        }
    }
}
