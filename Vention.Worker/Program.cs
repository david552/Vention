using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vention.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<GrpcSettings>(builder.Configuration.GetSection("Grpc"));

builder.Services.AddHostedService<UserFetchBackgroundService>();

var host = builder.Build();
host.Run();