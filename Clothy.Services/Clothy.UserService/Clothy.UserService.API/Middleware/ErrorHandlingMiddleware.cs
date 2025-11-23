using Clothy.Shared.Helpers.Exceptions;
using Clothy.UserService.BLL.Exceptions;
using System.Net;
using System.Text.Json;

namespace Clothy.UserService.API.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
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

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode status;
            string message;

            switch (exception)
            {
                case NotFoundException:
                    status = HttpStatusCode.NotFound;
                    message = exception.Message;
                    break;
                case AlreadyExistsException:
                    status = HttpStatusCode.Conflict;
                    message = exception.Message;
                    break;
                case IdentityOperationException:
                    status = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    break;
                case UnauthorizedAccessException:
                    status = HttpStatusCode.Unauthorized;
                    message = exception.Message;
                    break;
                case ValidationFailedException validationEx:
                    status = HttpStatusCode.BadRequest;
                    message = validationEx.Message;
                    break;
                default:
                    status = HttpStatusCode.InternalServerError;
                    message = "An unexpected error occurred.";
                    break;
            }

            _logger.LogError(exception, "Error occurred: {Message}", message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;

            var response = new
            {
                error = message,
                type = context.GetType().Name
            };

            string json = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });

            return context.Response.WriteAsync(json);
        }
    }
}
