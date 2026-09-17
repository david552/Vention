using System.Net.Http.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Vention.Application.Abstractions.Llm;
using Vention.Application.Options;

namespace Vention.Infrastructure.Services.Ollama
{
    public sealed class OllamaEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly OllamaOptions _options;
        private readonly IOllamaRequestGate _gate;
        private readonly ILogger<OllamaEmbeddingService> _logger;

        public OllamaEmbeddingService(
            HttpClient httpClient,
            IOptions<OllamaOptions> options,
            IOllamaRequestGate gate,
            ILogger<OllamaEmbeddingService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _gate = gate;
            _logger = logger;
        }

        public async Task<IReadOnlyList<float[]>> EmbedAsync(
            IReadOnlyList<string> inputs,
            CancellationToken ct = default)
        {
            if (inputs is null || inputs.Count == 0)
                throw new ArgumentException("At least one input is required.", nameof(inputs));

            var request = new EmbedRequest(_options.EmbeddingModel, inputs);

            _logger.LogDebug(
                "Requesting embeddings from Ollama. Model={Model}, Count={Count}",
                _options.EmbeddingModel,
                inputs.Count);

            using var lease = await _gate.AcquireAsync(ct);

            using var response = await _httpClient.PostAsJsonAsync("/api/embed", request, ct);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<EmbedResponse>(ct)
                ?? throw new InvalidOperationException("Ollama returned an empty embedding response.");


            if (body.Embeddings is null || body.Embeddings.Count == 0)
                throw new InvalidOperationException("Ollama returned no embeddings.");

            if (body.Embeddings.Count != inputs.Count)
            {
                throw new InvalidOperationException(
                    $"Expected {inputs.Count} embeddings but received {body.Embeddings.Count}.");
            }

            foreach (var embedding in body.Embeddings)
            {
                if (embedding is null || embedding.Length != _options.EmbeddingDimensions)
                {
                    throw new InvalidOperationException(
                        $"Embedding length mismatch. Expected {_options.EmbeddingDimensions}, got {embedding?.Length ?? 0}. " +
                        "Check Ollama:EmbeddingModel vs Ollama:EmbeddingDimensions.");
                }
            }

            return body.Embeddings;
        }

        private sealed record EmbedRequest(
             string Model,
             IReadOnlyList<string> Input);

        private sealed record EmbedResponse(
             List<float[]>? Embeddings);
    }
}