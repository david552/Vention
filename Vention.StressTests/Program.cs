using NBomber.CSharp;
using NBomber.Http.CSharp;
using System.Net.Http.Json;
using System.Text.Json;


var baseUrl = "http://localhost:5258";

var email = "david.piranishvili@vention.com";

var password = "User1234!";

var httpClient = Http.CreateDefaultClient();

string? accessToken = null;

var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};

var scenario = Scenario.Create("stress_get_orgs", async context =>
{
    if (string.IsNullOrWhiteSpace(accessToken))
        return Response.Fail(statusCode: "NO_TOKEN", message: "Access token was not initialized");

    var request = Http.CreateRequest("GET", $"{baseUrl}/orgs")
        .WithHeader("Authorization", $"Bearer {accessToken}")
        .WithHeader("Accept", "application/json")
        .WithHeader("X-Correlation-ID", Guid.NewGuid().ToString("D"));

    var response = await Http.Send(httpClient, request);
    return response;
})
.WithInit(async context =>
{
    context.Logger.Information("Logging in to {BaseUrl} as {Email}...", baseUrl, email);

    using var loginClient = new HttpClient { BaseAddress = new Uri(baseUrl) };

    var loginResponse = await loginClient.PostAsJsonAsync(
        "/auth/login",
        new LoginRequest(email, password));

    var body = await loginResponse.Content.ReadAsStringAsync();

    if (!loginResponse.IsSuccessStatusCode)
    {
        throw new InvalidOperationException(
            $"Login failed ({(int)loginResponse.StatusCode}): {body}");
    }

    var auth = JsonSerializer.Deserialize<AuthResponseDto>(body, jsonOptions)
               ?? throw new InvalidOperationException("Login returned empty/invalid JSON.");

    if (string.IsNullOrWhiteSpace(auth.AccessToken))
        throw new InvalidOperationException("Login succeeded but AccessToken was empty.");

    accessToken = auth.AccessToken;
    context.Logger.Information(
        "Login OK. UserId={UserId}, Memberships={Count}",
        auth.Id,
        auth.Memberships?.Count ?? 0);

    using var probe = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/orgs");
    probe.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
    var probeResponse = await httpClient.SendAsync(probe);
    var probeBody = await probeResponse.Content.ReadAsStringAsync();

    if (!probeResponse.IsSuccessStatusCode)
    {
        throw new InvalidOperationException(
            $"Probe GET /orgs failed ({(int)probeResponse.StatusCode}): {probeBody}");
    }

    context.Logger.Information("Probe GET /orgs OK. Starting stress scenario...");
})
.WithWarmUpDuration(TimeSpan.FromSeconds(5))
.WithLoadSimulations(
    Simulation.Inject(
        rate: 10,                              
        interval: TimeSpan.FromSeconds(1),
        during: TimeSpan.FromSeconds(30)),


    Simulation.RampingInject(
        rate: 100,                              
        interval: TimeSpan.FromSeconds(1),
        during: TimeSpan.FromSeconds(60)),

    Simulation.Inject(
        rate: 100,
        interval: TimeSpan.FromSeconds(1),
        during: TimeSpan.FromSeconds(45)),

    Simulation.RampingInject(
        rate: 200,
        interval: TimeSpan.FromSeconds(1),
        during: TimeSpan.FromSeconds(45)),

    Simulation.Inject(
        rate: 200,
        interval: TimeSpan.FromSeconds(1),
        during: TimeSpan.FromSeconds(30)),

    Simulation.RampingInject(
        rate: 0,
        interval: TimeSpan.FromSeconds(1),
        during: TimeSpan.FromSeconds(45)),

    Simulation.Inject(
        rate: 10,
        interval: TimeSpan.FromSeconds(1),
        during: TimeSpan.FromSeconds(30))
);


NBomberRunner
    .RegisterScenarios(scenario)
    .WithReportFolder("stress-reports")
    .WithReportFileName($"vention-9.4-get-orgs-{DateTime.Now:yyyyMMdd-HHmmss}")
    .Run();

Console.WriteLine();
Console.WriteLine("Done. Open the HTML report under ./stress-reports/");

internal sealed record LoginRequest(string Email, string Password);

internal sealed class AuthResponseDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Role { get; set; }
    public string? AccessToken { get; set; }
    public DateTimeOffset AccessTokenExpiresAt { get; set; }
    public List<AuthMembershipDto>? Memberships { get; set; }
}

internal sealed class AuthMembershipDto
{
    public Guid OrganisationId { get; set; }
    public string? OrganisationName { get; set; }
    public string? Role { get; set; }
}