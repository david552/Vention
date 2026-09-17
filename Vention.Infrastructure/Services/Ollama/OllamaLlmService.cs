using System.Net.Http.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Vention.Application.Abstractions.Llm;
using Vention.Application.Options;

namespace Vention.Infrastructure.Services.Ollama
{
    public sealed class OllamaLlmService : ILlmService
    {
        private readonly HttpClient _httpClient;
        private readonly OllamaOptions _options;
        private readonly IOllamaRequestGate _gate;
        private readonly ILogger<OllamaLlmService> _logger;

        public OllamaLlmService(
            HttpClient httpClient,
            IOptions<OllamaOptions> options,
            IOllamaRequestGate gate,
            ILogger<OllamaLlmService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _gate = gate;
            _logger = logger;
        }

        public async Task<string> ChatAsync(
            string systemPrompt,
            string userPrompt,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userPrompt))
                throw new ArgumentException("User prompt cannot be empty.", nameof(userPrompt));

            var messages = new List<ChatMessageDto>();

            if (!string.IsNullOrWhiteSpace(systemPrompt))
                messages.Add(new ChatMessageDto("system", systemPrompt));

            messages.Add(new ChatMessageDto("user", userPrompt));

            var request = new ChatRequest(_options.ChatModel, messages, Stream: false);

            _logger.LogDebug(
                "Requesting chat completion from Ollama. Model={Model}",
                _options.ChatModel);

            using var lease = await _gate.AcquireAsync(ct);

            using var response = await _httpClient.PostAsJsonAsync("/api/chat", request, ct);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<ChatResponse>(ct)
                ?? throw new InvalidOperationException("Ollama returned an empty chat response.");

            var content = body.Message?.Content;
            if (string.IsNullOrWhiteSpace(content))
                throw new InvalidOperationException("Ollama returned an empty assistant message.");

            return content;
        }

        private sealed record ChatMessageDto(
            string Role,
            string Content);

        private sealed record ChatRequest(
            string Model,
            IReadOnlyList<ChatMessageDto> Messages,
            bool Stream);

        private sealed record ChatResponse(
            ChatMessageDto? Message);
    }
}