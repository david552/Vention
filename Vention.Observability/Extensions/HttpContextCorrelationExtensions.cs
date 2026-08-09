using Microsoft.AspNetCore.Http;

namespace Vention.Observability.Extensions
{

    public static class HttpContextCorrelationExtensions
    {
        public static string? GetCorrelationId(this HttpContext httpContext)
        {
            if (httpContext.Items.TryGetValue(CorrelationIdConstants.HttpContextItemKey, out var value)
                && value is string s
                && !string.IsNullOrWhiteSpace(s))
            {
                return s;
            }
            return CorrelationIdContext.TryGetFromActivity();
        }
    }
}