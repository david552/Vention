using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Vention.Observability
{

    public static class CorrelationIdContext
    {
        public static string ResolveOrCreate(string? incoming)
        {
            if (!string.IsNullOrWhiteSpace(incoming))
                return incoming.Trim();

            var traceId = Activity.Current?.TraceId.ToString();
            if (!string.IsNullOrWhiteSpace(traceId) && traceId != default(ActivityTraceId).ToString())
                return traceId;

            return Guid.NewGuid().ToString("D");
        }

        public static void SetOnActivity(string correlationId)
        {
            var activity = Activity.Current;
            if (activity is null)
                return;

            activity.SetTag(CorrelationIdConstants.BaggageKey, correlationId);
            activity.SetBaggage(CorrelationIdConstants.BaggageKey, correlationId);
        }

        public static string? TryGetFromActivity()
        {
            var activity = Activity.Current;
            if (activity is null)
                return null;

            var fromBaggage = activity.GetBaggageItem(CorrelationIdConstants.BaggageKey);
            if (!string.IsNullOrWhiteSpace(fromBaggage))
                return fromBaggage;

            var tag = activity.GetTagItem(CorrelationIdConstants.BaggageKey)?.ToString();
            return string.IsNullOrWhiteSpace(tag) ? null : tag;
        }

        public static IDisposable BeginLogScope(ILogger logger, string correlationId)
        {
            return logger.BeginScope(new Dictionary<string, object>
            {
                [CorrelationIdConstants.LogScopeKey] = correlationId
            }) ?? NullScope.Instance;
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}