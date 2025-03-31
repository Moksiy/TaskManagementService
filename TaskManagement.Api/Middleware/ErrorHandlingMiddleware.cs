using System.Net;
using System.Text.Json;

namespace TaskManagement.Api.Middleware
{
    /// <summary>
    /// Middleware for global exception handling
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// Creates a new error handling middleware
        /// </summary>
        /// <param name="next">The next middleware in the pipeline</param>
        /// <param name="logger">Logger for error information</param>
        /// <param name="environment">Web host environment</param>
        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        /// <param name="context">HTTP context</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            var statusCode = HttpStatusCode.InternalServerError;
            var errorResponse = new ErrorResponse
            {
                Status = (int)statusCode,
                Title = "An error occurred while processing your request",
                TraceId = context.TraceIdentifier
            };

            if (_environment.IsDevelopment())
            {
                errorResponse.Detail = exception.Message;
                errorResponse.StackTrace = exception.StackTrace;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, jsonOptions));
        }
    }    
}