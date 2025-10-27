using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Clothy.ServiceDefaults.Middleware
{
    public class CorrelationIdDelegatingHandler : DelegatingHandler
    {
        private IHttpContextAccessor httpContextAccessor;
        private const string CORRELATION_ID_HEADER = "X-Correlation-Id";

        public CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string? correlationId = httpContextAccessor.HttpContext?.Items[CORRELATION_ID_HEADER]?.ToString();

            if (!string.IsNullOrEmpty(correlationId) && !request.Headers.Contains(CORRELATION_ID_HEADER))
            {
                request.Headers.TryAddWithoutValidation(CORRELATION_ID_HEADER, correlationId);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}