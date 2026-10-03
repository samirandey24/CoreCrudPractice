namespace CoreCrudWithJwt.Exception
{
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Mvc;

    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            System.Exception exception,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(httpContext);
            ArgumentNullException.ThrowIfNull(exception);
            // 1. Log the error internally
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            // 2. Map specific exceptions to HTTP Status Codes
            var (statusCode, title) = exception switch
            {
                ArgumentNullException => (StatusCodes.Status406NotAcceptable, "Status Not Acceptable"),
                InvalidOperationException => (StatusCodes.Status405MethodNotAllowed, "When an operation isn’t valid for the current state"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized access"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict"),// those exception not supported .net8 onwards  upto 431
                ArgumentException => (StatusCodes.Status400BadRequest, "Invalid arguments provided"),
                _ => (StatusCodes.Status500InternalServerError, "A server error occurred")
            };

            httpContext.Response.StatusCode = statusCode;

            // 3. Create a standardized Problem Details payload
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            };

            // 4. Write directly to the HTTP response stream
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            // Return true to signal that the exception has been completely handled
            return true;
        }
        
    }
    public class ConflictException : System.Exception
    {
        public ConflictException(string message)
            : base(message)
        {
            throw new ConflictException("The email already exists.");

        }
    }


}
