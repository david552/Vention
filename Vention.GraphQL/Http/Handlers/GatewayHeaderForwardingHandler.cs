namespace Vention.GraphQL.Http.Handlers
{

    public sealed class GatewayHeaderForwardingHandler : DelegatingHandler
    {
        public const string UserIdHeader = "X-User-Id";
        public const string GatewaySecretHeader = "X-Gateway-Secret";
        public const string CorrelationHeader = "X-Correlation-ID";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public GatewayHeaderForwardingHandler(IHttpContextAccessor httpContextAccessor)
            => _httpContextAccessor = httpContextAccessor;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var incoming = _httpContextAccessor.HttpContext?.Request
                ?? throw new InvalidOperationException("No HTTP context to forward gateway headers.");

            Copy(incoming, request, UserIdHeader);
            Copy(incoming, request, GatewaySecretHeader);
            Copy(incoming, request, CorrelationHeader);

            return base.SendAsync(request, cancellationToken);
        }

        private static void Copy(HttpRequest incoming, HttpRequestMessage outgoing, string name)
        {
            var value = incoming.Headers[name].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(value))
                return;

            outgoing.Headers.Remove(name);
            outgoing.Headers.TryAddWithoutValidation(name, value);
        }
    }
}