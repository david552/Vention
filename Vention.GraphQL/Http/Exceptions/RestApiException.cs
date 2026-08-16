namespace Vention.GraphQL.Http.Exceptions
{

    public sealed class RestApiException : Exception
    {
        public int StatusCode { get; }
        public string? Code { get; }

        public RestApiException(int statusCode, string message, string? code = null)
            : base(message)
        {
            StatusCode = statusCode;
            Code = code;
        }
    }
}