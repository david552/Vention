using Grpc.Net.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vention.API.Protos;

namespace Vention.Worker;

public sealed class UserFetchBackgroundService : BackgroundService
{
    private readonly ILogger<UserFetchBackgroundService> _logger;
    private readonly GrpcSettings _grpcSettings;

    public UserFetchBackgroundService(
        ILogger<UserFetchBackgroundService> logger,
        IOptions<GrpcSettings> grpcOptions)
    {
        _logger = logger;
        _grpcSettings = grpcOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var address = _grpcSettings.ApiAddress;
        var userId = _grpcSettings.UserId;

        if (string.IsNullOrEmpty(address) || string.IsNullOrEmpty(userId))
        {
            throw new InvalidOperationException("Grpc configuration is missing required values.");
        }

        using var channel = GrpcChannel.ForAddress(address);
        var client = new UserService.UserServiceClient(channel);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await client.GetUserByIdAsync(
                    new GetUserByIdRequest
                    {
                        UserId = userId,
                        ActingUserId = userId 
                    },
                    cancellationToken: stoppingToken);

                _logger.LogInformation(
                    "gRPC GetUserById OK: {Id} | {Email} | {Name}",
                    response.Id,
                    response.Email,
                    response.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "gRPC GetUserById failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}