using HotChocolate.Execution;
using Vention.GraphQL.Http.Exceptions;

namespace Vention.GraphQL.ErrorHandling
{

    public sealed class GraphQLErrorFilter : IErrorFilter
    {
        public IError OnError(IError error)
        {
            if (error.Exception is RestApiException ex)
                return error.WithCode(ex.Code ?? "REST_ERROR").WithMessage(ex.Message);

            return error;
        }
    }
}