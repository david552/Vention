using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Formatting.Compact;
using Vention.API.Consumers;
using Vention.API.ExceptionHandlers;
using Vention.API.Extensions;
using Vention.API.GrpcServices;
using Vention.API.Interceptors;
using Vention.Application;
using Vention.Application.Options;
using Vention.Infrastructure;
using Vention.Infrastructure.Messaging;
using Vention.Observability.Extensions;
using Vention.Presentation.Common.Extensions;
using Vention.Presentation.Common.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithSpan()  
    .WriteTo.Console(new CompactJsonFormatter()));


builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "Vention.API",
            serviceVersion: "1.0.0",
            serviceInstanceId: Environment.GetEnvironmentVariable("INSTANCE_NAME") ?? Environment.MachineName))
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

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services
    .AddControllers()
    .AddVentionJsonOptions();

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<GrpcExceptionInterceptor>();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddOptions<FileStorageSettingsOptions>()
    .Bind(builder.Configuration.GetSection("FileStorage"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<CryptoSettingsOptions>()
    .Bind(builder.Configuration.GetSection("CryptoSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<RabbitMqSettingsOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMqSettingsOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<SignalRRedisOptions>()
    .Bind(builder.Configuration.GetSection(SignalRRedisOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddApplicationServices();

builder.Services.AddInfrastructure(
    builder.Configuration,
    MassTransitHostKind.Api,
    typeof(FileStatusChangedConsumer).Assembly);


builder.Services.AddSwaggerWithAuth();
builder.Services.AddJwtSettings(builder.Configuration);
builder.Services.AddVentionRateLimiting();
builder.Services.AddPresentationGatewayAuth(builder.Configuration);

builder.Services.AddVentionAuthorization();

builder.Services.AddVentionSignalR(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GatewayTrustMiddleware>();

app.UseAuthentication();
app.UseAuthorization();


//app.UseHttpsRedirection();
//app.UseCorrelationId();

// OpenTelemetry: Serilog request logging with TraceId
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
    };
});
// OpenTelemetry: Expose /metrics endpoint for Prometheus
app.MapPrometheusScrapingEndpoint().AllowAnonymous();


app.UseRateLimiter();

app.MapControllers();


app.MapGrpcService<UserGrpcService>();

app.MapVentionHubs();

await app.SeedDatabaseAsync();

app.Run();
