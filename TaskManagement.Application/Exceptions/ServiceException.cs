
namespace TaskManagement.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a service operation fails
    /// </summary>
    public class ServiceException : Exception
    {
        /// <summary>
        /// Creates a new ServiceException
        /// </summary>
        /// <param name="message">Exception message</param>
        public ServiceException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new ServiceException with an inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public ServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
