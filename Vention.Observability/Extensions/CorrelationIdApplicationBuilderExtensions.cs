using Microsoft.AspNetCore.Builder;
using Vention.Observability.Middleware;

namespace Vention.Observability.Extensions
{

    public static class CorrelationIdApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
            => app.UseMiddleware<CorrelationIdMiddleware>();
    }
}