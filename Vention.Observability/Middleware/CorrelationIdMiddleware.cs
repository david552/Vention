using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace Vention.Observability.Middleware;

public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var incoming = context.Request.Headers[CorrelationIdConstants.HeaderName].FirstOrDefault();
        var correlationId = CorrelationIdContext.ResolveOrCreate(incoming);

        CorrelationIdContext.SetOnActivity(correlationId);
        context.Items[CorrelationIdConstants.HttpContextItemKey] = correlationId;
        context.TraceIdentifier = correlationId; 

        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdConstants.HeaderName))
                context.Response.Headers[CorrelationIdConstants.HeaderName] = correlationId;

            return Task.CompletedTask;
        });

        using (CorrelationIdContext.BeginLogScope(_logger, correlationId))
        {
            await _next(context);
        }
    }
}