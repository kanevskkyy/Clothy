
using Clothy.Shared.Helpers.Exceptions;
using MongoDB.Driver;
using System.Net;

namespace Clothy.ReviewService.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private RequestDelegate next;
        private ILogger<ExceptionHandlingMiddleware> logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception caught by middleware.");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = ex switch
                {
                    NotFoundException => (int)HttpStatusCode.NotFound,
                    AlreadyExistsException => (int)HttpStatusCode.Conflict,
                    MongoConnectionException => (int)HttpStatusCode.ServiceUnavailable,
                    MongoWriteException => (int)HttpStatusCode.BadRequest,
                    ValidationFailedException => (int)HttpStatusCode.BadRequest,
                    MongoException => (int)HttpStatusCode.ServiceUnavailable,

                    _ => (int)HttpStatusCode.InternalServerError
                };

                var response = new
                {
                    type = ex.GetType().Name,
                    message = ex.Message,
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}