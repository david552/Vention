using System.IdentityModel.Tokens.Jwt;
using Vention.API.ExceptionHandlers;
using Vention.API.Extensions;
using Vention.Application;
using Vention.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services
    .AddControllers()
    .AddVentionJsonOptions();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddOptions<CryptoSettingsOptions>()
    .Bind(builder.Configuration.GetSection("CryptoSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);



builder.Services.AddSwaggerWithAuth();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddVentionRateLimiting();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler(); 

app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.SeedDatabaseAsync();

app.Run();
