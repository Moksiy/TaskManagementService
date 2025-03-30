using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Domain.Events
{
    /// <summary>
    /// Base class for all task-related events
    /// </summary>
    public abstract class TaskEvent
    {
        /// <summary>
        /// The ID of the task this event relates to
        /// </summary>
        public Guid TaskId { get; }

        /// <summary>
        /// Constructor for the base task event
        /// </summary>
        /// <param name="taskId">ID of the task</param>
        protected TaskEvent(Guid taskId)
        {
            TaskId = taskId;
        }
    }
}
