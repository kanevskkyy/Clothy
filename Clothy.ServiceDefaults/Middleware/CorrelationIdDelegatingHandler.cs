using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Clothy.ServiceDefaults.Middleware
{
    public class CorrelationIdDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CorrelationIdHeader = "X-Correlation-Id";

        public CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var correlationId = _httpContextAccessor.HttpContext?.Items[CorrelationIdHeader]?.ToString();

            if (!string.IsNullOrEmpty(correlationId) && !request.Headers.Contains(CorrelationIdHeader))
            {
                request.Headers.TryAddWithoutValidation(CorrelationIdHeader, correlationId);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}