using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Formatting.Compact;

using Vention.Worker;

var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .WriteTo.Console(new CompactJsonFormatter()));


builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "Vention.Worker",
            serviceVersion: "1.0.0",
            serviceInstanceId: Environment.MachineName))
    .WithMetrics(metrics => metrics
        .AddRuntimeInstrumentation()
        .AddPrometheusHttpListener(options => options.UriPrefixes = new[] { "http://localhost:9465/" }));

builder.Services.Configure<GrpcSettings>(builder.Configuration.GetSection("Grpc"));

builder.Services.AddHostedService<UserFetchBackgroundService>();

var host = builder.Build();
host.Run();