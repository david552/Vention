using Microsoft.Extensions.Options;
using Vention.GraphQL.Extensions;
using Vention.GraphQL.Http.Clients;
using Vention.GraphQL.Http.Handlers;
using Vention.GraphQL.Http.Options;
using Vention.Observability.Extensions;
using Vention.Presentation.Common.Extensions;
using Vention.Presentation.Common.Middleware;

var builder = WebApplication.CreateBuilder(args);

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

app.MapGraphQL("/graphql");

app.Run();