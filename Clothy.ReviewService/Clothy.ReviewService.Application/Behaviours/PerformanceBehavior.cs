using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Clothy.ReviewService.Application.Behaviours
{
    public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private ILogger<PerformanceBehavior<TRequest, TResponse>> logger;

        public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
        {
            this.logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            TResponse response = await next();
            stopwatch.Stop();

            long elapsedMs = stopwatch.ElapsedMilliseconds;
            if (elapsedMs > 500)
            {
                logger.LogWarning("[PERFORMANCE] {RequestName} took {ElapsedMilliseconds}ms", typeof(TRequest).Name, elapsedMs);
            }
            else
            {
                logger.LogInformation("[PERFORMANCE] {RequestName} executed in {ElapsedMilliseconds}ms", typeof(TRequest).Name, elapsedMs);
            }

            return response;
        }
    }
}
