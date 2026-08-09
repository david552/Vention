using MassTransit;
using Microsoft.Extensions.Logging;
using Vention.Observability;

namespace Vention.Infrastructure.Messaging.Filters
{

    public sealed class CorrelationIdConsumeFilter<T> : IFilter<ConsumeContext<T>>
        where T : class
    {
        private readonly ILogger<CorrelationIdConsumeFilter<T>> _logger;

        public CorrelationIdConsumeFilter(ILogger<CorrelationIdConsumeFilter<T>> logger)
        {
            _logger = logger;
        }

        public void Probe(ProbeContext context) =>
            context.CreateFilterScope("correlation-id-consume");

        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            var incoming = context.Headers.Get<string>(CorrelationIdConstants.HeaderName);
            var correlationId = CorrelationIdContext.ResolveOrCreate(incoming);

            CorrelationIdContext.SetOnActivity(correlationId);

            using (CorrelationIdContext.BeginLogScope(_logger, correlationId))
            {
                await next.Send(context);
            }
        }
    }
}