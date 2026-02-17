using Clothy.Shared.Helpers.Exceptions;
using Grpc.Core;
using System.Net;

namespace Clothy.Aggregator.API.Middleware
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
            catch (RpcException rpcEx)
            {
                logger.LogWarning(rpcEx, "gRPC exception: {StatusCode} - {Detail}", rpcEx.StatusCode, rpcEx.Status.Detail);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = rpcEx.StatusCode switch
                {
                    StatusCode.NotFound => (int)HttpStatusCode.NotFound,
                    StatusCode.InvalidArgument => (int)HttpStatusCode.BadRequest,
                    StatusCode.Unauthenticated => (int)HttpStatusCode.Unauthorized,
                    StatusCode.PermissionDenied => (int)HttpStatusCode.Forbidden,
                    StatusCode.AlreadyExists => (int)HttpStatusCode.Conflict,
                    StatusCode.Cancelled => (int)HttpStatusCode.RequestTimeout,
                    StatusCode.Unavailable => (int)HttpStatusCode.ServiceUnavailable,
                    _ => (int)HttpStatusCode.InternalServerError
                };

                var response = new
                {
                    error = rpcEx.Status.Detail,
                    type = "RpcException",
                };

                await context.Response.WriteAsJsonAsync(response);
            }
            catch (NotFoundException notFoundEx)
            {
                logger.LogWarning(notFoundEx, "Resource not found: {Message}", notFoundEx.Message);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                var response = new
                {
                    error = notFoundEx.Message,
                    type = "NotFoundException"
                };

                await context.Response.WriteAsJsonAsync(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception caught by middleware.");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

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