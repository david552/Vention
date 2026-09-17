using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;
using Vention.Application.Abstractions.Llm;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Application.Options;
using Vention.Application.Rag.Contracts;
using Vention.Domain.DocumentChunks;
using Vention.Domain.Files;
using Vention.Domain.Organizations;

namespace Vention.Application.Rag.Commands.AskQuestion
{
    public sealed class AskQuestionCommandHandler
        : ICommandHandler<AskQuestionCommand, AskQuestionResponse>
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly ILlmService _llmService;
        private readonly IDocumentChunkRepository _documentChunkRepository;
        private readonly IStoredFileRepository _storedFileRepository;
        private readonly RagOptions _ragOptions;

        public AskQuestionCommandHandler(
            IEmbeddingService embeddingService,
            ILlmService llmService,
            IDocumentChunkRepository documentChunkRepository,
            IStoredFileRepository storedFileRepository,
            IOptions<RagOptions> ragOptions)
        {
            _embeddingService = embeddingService;
            _llmService = llmService;
            _documentChunkRepository = documentChunkRepository;
            _storedFileRepository = storedFileRepository;
            _ragOptions = ragOptions.Value;
        }

        public async Task<AskQuestionResponse> Handle(AskQuestionCommand command, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();

            var question = command.Question.Trim();
            StoredFileId? fileId = null;

            if (command.FileId is Guid selectedFileId)
            {
                var file = await _storedFileRepository.GetByIdAsync(new StoredFileId(selectedFileId), ct)
                    ?? throw new NotFoundException($"File '{selectedFileId}' was not found.");

                if (file.OrganizationId.Value != command.OrganizationId)
                    throw new NotFoundException($"File '{selectedFileId}' was not found.");

                if (file.Status != FileStatus.Processed)
                    throw new NotFoundException($"File '{selectedFileId}' is not ready for questions (status: {file.Status}).");

                fileId = file.Id;
            }

            var embeddings = await _embeddingService.EmbedAsync(new[] { question }, ct);
            var queryEmbedding = embeddings[0];

            var chunks = await _documentChunkRepository.SearchSimilarAsync(
                new OrganizationId(command.OrganizationId),
                queryEmbedding,
                _ragOptions.TopK,
                fileId,
                _ragOptions.MaxCosineDistance,
                ct);

            if (chunks.Count == 0)
            {
                var emptyMessage = fileId is null
                    ? "I could not find relevant information in the uploaded documents."
                    : "I could not find relevant information in the selected file.";

                sw.Stop();
                RagMetrics.RecordAsk(sw.Elapsed.TotalMilliseconds, 0, "no_hits");
                return new AskQuestionResponse(emptyMessage, Array.Empty<AskQuestionSource>());
            }

            var contextChunks = SelectContextChunks(chunks, _ragOptions.MaxContextChars);

            var contextBuilder = new StringBuilder();
            for (var i = 0; i < contextChunks.Count; i++)
            {
                var chunk = contextChunks[i];
                contextBuilder.AppendLine($"[Source {i + 1}] file={chunk.FileId.Value}, chunk={chunk.ChunkIndex}");
                contextBuilder.AppendLine(chunk.Text);
                contextBuilder.AppendLine();
            }

            const string systemPrompt =
                "You are an assistant for question-answering tasks. Use the provided context to answer the question. " +
                "If a piece of context is irrelevant, ignore it. " +
                "If the context is insufficient to answer, say you do not know based on the uploaded documents. " +
                "When possible, refer to sources by their numbers (e.g. Source 1). " +
                "Answer in the same language as the user's question.";

            var userPrompt =
                $"Context:\n{contextBuilder}\nQuestion: {question}\n\nAnswer:";

            var answer = await _llmService.ChatAsync(systemPrompt, userPrompt, ct);

            var snippetChars = _ragOptions.SourceSnippetChars;

            var sources = contextChunks
                .Select(c => new AskQuestionSource(
                    c.FileId.Value,
                    c.ChunkIndex,
                    Truncate(c.Text, snippetChars)))
                .ToList();

            sw.Stop();
            RagMetrics.RecordAsk(sw.Elapsed.TotalMilliseconds, contextChunks.Count, "success");
            return new AskQuestionResponse(answer, sources);
        }

        private static IReadOnlyList<DocumentChunk> SelectContextChunks(
            IReadOnlyList<DocumentChunk> chunks,
            int maxContextChars)
        {
            if (maxContextChars <= 0)
                return Array.Empty<DocumentChunk>();

            var selected = new List<DocumentChunk>(chunks.Count);
            var used = 0;

            foreach (var chunk in chunks)
            {
                var length = chunk.Text.Length;

                if (selected.Count > 0 && used + length > maxContextChars)
                    break;

                selected.Add(chunk);
                used += length;
            }

            return selected;
        }
        private static string Truncate(string text, int maxChars)
        {
            if (maxChars <= 0)
                return string.Empty;

            if (string.IsNullOrEmpty(text) || text.Length <= maxChars)
                return text;

            return text[..maxChars].TrimEnd() + "…";
        }
    }
}