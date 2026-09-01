namespace Vention.GraphQL.Http.Options
{

    public sealed class RestApiOptions
    {
        public const string SectionName = "RestApi";

        public string BaseAddress { get; set; } = "http://localhost:5259/";
    }
}