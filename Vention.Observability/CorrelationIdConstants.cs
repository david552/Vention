namespace Vention.Observability
{

    public static class CorrelationIdConstants
    {
        public const string HeaderName = "X-Correlation-ID";

        public const string BaggageKey = "correlation.id";

        public const string LogScopeKey = "CorrelationId";

        public const string HttpContextItemKey = "CorrelationId";
    }
}