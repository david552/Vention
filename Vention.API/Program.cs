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


//app.UseHttpsRedirection();
app.UseCorrelationId();


app.UseRateLimiter();

app.MapControllers();


app.MapGrpcService<UserGrpcService>();

app.MapVentionHubs();

await app.SeedDatabaseAsync();

app.Run();
