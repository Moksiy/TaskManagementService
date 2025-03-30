using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Domain.Events
{
    /// <summary>
    /// Event raised when a task is deleted
    /// </summary>
    public class TaskDeletedEvent : TaskEvent
    {
        /// <summary>
        /// Creates a new TaskDeletedEvent
        /// </summary>
        /// <param name="taskId">ID of the deleted task</param>
        public TaskDeletedEvent(Guid taskId) : base(taskId)
        {
        }
    }
}
