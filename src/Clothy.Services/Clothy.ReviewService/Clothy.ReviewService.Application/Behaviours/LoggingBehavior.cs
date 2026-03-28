using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Clothy.ReviewService.Application.Behaviours
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private ILogger<LoggingBehavior<TRequest, TResponse>> logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            this.logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            string requestId = Guid.NewGuid().ToString();
            logger.LogInformation("[START] {RequestName} ({RequestId}) - {Request}", typeof(TRequest).Name, requestId, JsonSerializer.Serialize(request));

            TResponse response = await next();

            logger.LogInformation("[SUCCESS] {RequestName} ({RequestId}) completed", typeof(TRequest).Name, requestId);
            return response;
        }
    }
}
