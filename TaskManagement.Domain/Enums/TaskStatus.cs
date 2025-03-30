namespace TaskManagement.Domain.Enums
{
    /// <summary>
    /// Possible states of a task
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// Task is newly created
        /// </summary>
        New,

        /// <summary>
        /// Task is in progress
        /// </summary>
        InProgress,

        /// <summary>
        /// Task is completed
        /// </summary>
        Completed
    }
}
