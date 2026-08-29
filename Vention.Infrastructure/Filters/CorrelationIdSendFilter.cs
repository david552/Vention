using MassTransit;
using Vention.Observability;

namespace Vention.Infrastructure.Messaging.Filters
{

    public sealed class CorrelationIdSendFilter<T> : IFilter<SendContext<T>>
        where T : class
    {
        public void Probe(ProbeContext context) =>
            context.CreateFilterScope("correlation-id-send");

        public Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
        {
            var correlationId = CorrelationIdContext.TryGetFromActivity()
                ?? CorrelationIdContext.ResolveOrCreate(null);

            CorrelationIdContext.SetOnActivity(correlationId);
            context.Headers.Set(CorrelationIdConstants.HeaderName, correlationId);

            return next.Send(context);
        }
    }
}