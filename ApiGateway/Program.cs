using ApiGateway.Extensions;
using ApiGateway.Options;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Vention.Observability;
using Vention.Observability.Extensions;
using Yarp.ReverseProxy.Transforms;


JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddGatewayJwtAuthentication(builder.Configuration);

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024;
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 50 * 1024 * 1024;
});

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transformContext =>
    {
        transformContext.AddRequestTransform(async transform =>
        {
            transform.ProxyRequest.Headers.Remove("X-User-Id");
            transform.ProxyRequest.Headers.Remove("X-Gateway-Secret");
            await ValueTask.CompletedTask;
        });

        transformContext.AddRequestTransform(async transform =>
        {
            var correlationId = transform.HttpContext.GetCorrelationId()
                ?? transform.HttpContext.Request.Headers[CorrelationIdConstants.HeaderName].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                transform.ProxyRequest.Headers.Remove(CorrelationIdConstants.HeaderName);
                transform.ProxyRequest.Headers.TryAddWithoutValidation(
                    CorrelationIdConstants.HeaderName, correlationId);
            }

            await ValueTask.CompletedTask;
        });


        if (!string.Equals(transformContext.Route.AuthorizationPolicy, "Anonymous", StringComparison.OrdinalIgnoreCase))
        {
            transformContext.AddRequestTransform(async transform =>
            {
                var gatewayOptions = transform.HttpContext.RequestServices
                    .GetRequiredService<IOptions<GatewayOptions>>().Value;

                var userId = transform.HttpContext.User.FindFirstValue("sub")
                    ?? transform.HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? transform.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    transform.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Id", userId);
                    transform.ProxyRequest.Headers.TryAddWithoutValidation("X-Gateway-Secret", gatewayOptions.SharedSecret);
                }

                await ValueTask.CompletedTask;
            });
        }
    });



var app = builder.Build();

app.UseCorrelationId();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();

app.Run();
