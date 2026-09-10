using Microsoft.Extensions.Options;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Formatting.Compact;

using Vention.GraphQL.Extensions;
using Vention.GraphQL.Http.Clients;
using Vention.GraphQL.Http.Handlers;
using Vention.GraphQL.Http.Options;
using Vention.Observability.Extensions;
using Vention.Presentation.Common.Extensions;
using Vention.Presentation.Common.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithSpan()  // Automatically adds TraceId and SpanId to all logs
    .WriteTo.Console(new CompactJsonFormatter()));

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "Vention.GraphQL",
            serviceVersion: "1.0.0",
            serviceInstanceId: Environment.MachineName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.EnrichWithHttpRequest = (activity, httpRequest) =>
            {
                activity.SetTag("http.request.header.user-agent", httpRequest.Headers.UserAgent.ToString());
            };
            options.EnrichWithHttpResponse = (activity, httpResponse) =>
            {
                activity.SetTag("http.response.status_code", httpResponse.StatusCode);
            };
        })
        .AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());

builder.Services.AddPresentationGatewayAuth(builder.Configuration);

builder.Services.Configure<RestApiOptions>(
    builder.Configuration.GetSection(RestApiOptions.SectionName));

builder.Services.AddTransient<GatewayHeaderForwardingHandler>();

builder.Services.AddHttpClient<IVentionApiClient, VentionApiClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<RestApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseAddress);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<GatewayHeaderForwardingHandler>();

builder.Services.AddVentionGraphQL(builder.Environment);

var app = builder.Build();

app.UseMiddleware<GatewayTrustMiddleware>();
app.UseCorrelationId();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
    };
});

app.MapPrometheusScrapingEndpoint().AllowAnonymous();

app.MapGraphQL("/graphql");

app.Run();