using MassTransit;

namespace Vention.Processor.Consumers
{
    public sealed class PrepareFileIngestionConsumerDefinition
        : ConsumerDefinition<PrepareFileIngestionConsumer>
    {
        public PrepareFileIngestionConsumerDefinition()
        {
            ConcurrentMessageLimit = 1;
        }

        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<PrepareFileIngestionConsumer> consumerConfigurator,
            IRegistrationContext context)
        {
            endpointConfigurator.PrefetchCount = 1;
        }
    }
}