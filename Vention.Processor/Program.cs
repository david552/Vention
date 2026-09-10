using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Formatting.Compact;
using Vention.Application.Options;
using Vention.Infrastructure;
using Vention.Infrastructure.Messaging;
using Vention.Processor.Consumers;

var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .WriteTo.Console(new CompactJsonFormatter()));

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "Vention.Processor",
            serviceVersion: "1.0.0",
            serviceInstanceId: Environment.GetEnvironmentVariable("INSTANCE_NAME") ?? Environment.MachineName))
    .WithMetrics(metrics => metrics
        .AddRuntimeInstrumentation()
        .AddPrometheusHttpListener(options => options.UriPrefixes = new[] { "http://localhost:9464/" }));

builder.Services.AddOptions<RabbitMqSettingsOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMqSettingsOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<FileStorageSettingsOptions>()
    .Bind(builder.Configuration.GetSection("FileStorage"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddInfrastructure(
    builder.Configuration,
    MassTransitHostKind.Worker,
    typeof(PrepareFileIngestionConsumer).Assembly);

var host = builder.Build();
host.Run();