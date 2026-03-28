using Clothy.Shared.Helpers.Exceptions;
using System.Net;
using System.Text.Json;

namespace Clothy.BasketService.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private RequestDelegate _next;
        private ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception caught by middleware.");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = ex switch
                {
                    ValidationFailedException => (int)HttpStatusCode.BadRequest,
                    UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                    _ => (int)HttpStatusCode.InternalServerError
                };

                var response = new
                {
                    error = ex.Message,
                    type = ex.GetType().Name
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
