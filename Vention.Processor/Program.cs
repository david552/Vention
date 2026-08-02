using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vention.Application.Options;
using Vention.Infrastructure;
using Vention.Infrastructure.Messaging;
using Vention.Processor.Consumers;

var builder = Host.CreateApplicationBuilder(args);

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