using MassTransit;
using Vention.Observability;

namespace Vention.Infrastructure.Messaging.Filters
{

    public sealed class CorrelationIdPublishFilter<T> : IFilter<PublishContext<T>>
        where T : class
    {
        public void Probe(ProbeContext context) =>
            context.CreateFilterScope("correlation-id-publish");

        public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
        {
            var correlationId = CorrelationIdContext.TryGetFromActivity()
                ?? CorrelationIdContext.ResolveOrCreate(null);

            CorrelationIdContext.SetOnActivity(correlationId);

            context.Headers.Set(CorrelationIdConstants.HeaderName, correlationId);

            return next.Send(context);
        }
    }
}