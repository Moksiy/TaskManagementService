using FluentValidation;
using System.Net;

namespace TaskManagement.Api.Endpoints
{
    /// <summary>
    /// Endpoint filter for validating request models using FluentValidation
    /// </summary>
    /// <typeparam name="T">The type of request model to validate</typeparam>
    public class ValidationFilter<T> : IEndpointFilter where T : class
    {
        private readonly IValidator<T> _validator;

        /// <summary>
        /// Creates a new validation filter
        /// </summary>
        /// <param name="validator">Validator for request model</param>
        public ValidationFilter(IValidator<T> validator)
        {
            _validator = validator;
        }

        /// <summary>
        /// Invokes the filter to validate the request model
        /// </summary>
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var requestModel = context.Arguments.FirstOrDefault(a => a?.GetType() == typeof(T)) as T;

            if (requestModel == null)
            {
                return Results.BadRequest(new
                {
                    Errors = new[] { "Request model is null or invalid" }
                });
            }

            var validationResult = await _validator.ValidateAsync(requestModel);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray()
                    );

                return Results.ValidationProblem(errors);
            }

            return await next(context);
        }
    }
}