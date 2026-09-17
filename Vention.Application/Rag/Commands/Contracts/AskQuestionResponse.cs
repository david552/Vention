namespace Vention.Application.Rag.Contracts
{
    public sealed record AskQuestionResponse(
        string Answer,
        IReadOnlyList<AskQuestionSource> Sources);

    public sealed record AskQuestionSource(
        Guid FileId,
        int ChunkIndex,
        string Text);
}