using Microsoft.Extensions.Options;

using Vention.Application.Abstractions.Llm;
using Vention.Application.Options;

namespace Vention.Infrastructure.Services.Ollama
{
    public sealed class OllamaRequestGate : IOllamaRequestGate, IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        public OllamaRequestGate(IOptions<OllamaOptions> options)
        {
            var max = Math.Max(1, options.Value.MaxConcurrentRequests);
            _semaphore = new SemaphoreSlim(max, max);
        }

        public async Task<IDisposable> AcquireAsync(CancellationToken ct = default)
        {
            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            return new Releaser(_semaphore);
        }

        public void Dispose() => _semaphore.Dispose();

        private sealed class Releaser : IDisposable
        {
            private SemaphoreSlim? _semaphore;

            public Releaser(SemaphoreSlim semaphore) => _semaphore = semaphore;

            public void Dispose()
            {
                var semaphore = Interlocked.Exchange(ref _semaphore, null);
                semaphore?.Release();
            }
        }
    }
}
