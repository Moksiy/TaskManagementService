using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Domain.Events
{
    /// <summary>
    /// Event raised when a new task is created
    /// </summary>
    public class TaskCreatedEvent : TaskEvent
    {
        /// <summary>
        /// Title of the created task
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Description of the created task
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Creates a new TaskCreatedEvent
        /// </summary>
        /// <param name="taskId">ID of the created task</param>
        /// <param name="title">Title of the created task</param>
        /// <param name="description">Description of the created task</param>
        public TaskCreatedEvent(Guid taskId, string title, string description) : base(taskId)
        {
            Title = title;
            Description = description;
        }
    }
}
