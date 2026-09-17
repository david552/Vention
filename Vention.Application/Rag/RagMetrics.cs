using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Vention.Application.Rag
{
    public static class RagMetrics
    {
        public const string MeterName = "Vention.Rag";

        private static readonly Meter Meter = new(MeterName, "1.0.0");

        private static readonly Histogram<double> AskDurationMs =
            Meter.CreateHistogram<double>(
                "vention_rag_ask_duration_ms",
                unit: "ms",
                description: "RAG ask end-to-end duration in milliseconds.");

        private static readonly Histogram<int> AskChunksRetrieved =
            Meter.CreateHistogram<int>(
                "vention_rag_ask_chunks_retrieved",
                description: "Number of chunks used as LLM context for an ask.");

        private static readonly Counter<long> AskTotal =
            Meter.CreateCounter<long>(
                "vention_rag_ask_total",
                description: "RAG ask attempts by outcome.");

        private static readonly Histogram<double> IngestDurationMs =
            Meter.CreateHistogram<double>(
                "vention_rag_ingest_duration_ms",
                unit: "ms",
                description: "File ingestion prepare duration in milliseconds.");

        private static readonly Histogram<int> IngestChunks =
            Meter.CreateHistogram<int>(
                "vention_rag_ingest_chunks",
                description: "Chunks persisted for a successful ingestion.");

        private static readonly Counter<long> IngestTotal =
            Meter.CreateCounter<long>(
                "vention_rag_ingest_total",
                description: "Ingestion prepare attempts by outcome.");

        public static void RecordAsk(double durationMs, int contextChunkCount, string outcome)
        {
            AskDurationMs.Record(durationMs, new KeyValuePair<string, object?>("outcome", outcome));
            AskChunksRetrieved.Record(contextChunkCount);
            AskTotal.Add(1, new KeyValuePair<string, object?>("outcome", outcome));
        }

        public static void RecordIngest(double durationMs, int chunkCount, string outcome)
        {
            IngestDurationMs.Record(durationMs, new KeyValuePair<string, object?>("outcome", outcome));
            if (chunkCount > 0)
                IngestChunks.Record(chunkCount);
            IngestTotal.Add(1, new KeyValuePair<string, object?>("outcome", outcome));
        }
    }
}