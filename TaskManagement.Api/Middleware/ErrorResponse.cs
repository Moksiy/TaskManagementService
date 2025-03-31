namespace TaskManagement.Api.Middleware
{
    // <summary>
    /// Response model for errors
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// HTTP status code
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Error title
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Detailed error description (only in development)
        /// </summary>
        public string? Detail { get; set; }

        /// <summary>
        /// Request trace identifier
        /// </summary>
        public string TraceId { get; set; } = null!;

        /// <summary>
        /// Error stack trace (only in development)
        /// </summary>
        public string? StackTrace { get; set; }
    }
}
