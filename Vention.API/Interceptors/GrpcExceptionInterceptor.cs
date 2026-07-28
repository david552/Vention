using Grpc.Core;
using Grpc.Core.Interceptors;
using System.Diagnostics;
using Vention.Application.Exceptions;

namespace Vention.API.Interceptors
{
    public sealed class GrpcExceptionInterceptor : Interceptor
    {
        private readonly ILogger<GrpcExceptionInterceptor> _logger;
        private readonly IHostEnvironment _env;

        public GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                return await continuation(request, context);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception exception)
            {
                StatusCode statusCode;
                string detail;

                switch (exception)
                {
                    case ValidationException:
                        statusCode = StatusCode.InvalidArgument;
                        detail = "One or more validation errors occurred.";
                        break;

                    case NotFoundException nf:
                        statusCode = StatusCode.NotFound;
                        detail = nf.Message;
                        break;

                    case ArgumentException arg:
                        statusCode = StatusCode.InvalidArgument;
                        detail = arg.Message;
                        break;

                    case InvalidOperationException op:
                        statusCode = StatusCode.FailedPrecondition;
                        detail = op.Message;
                        break;

                    case UnauthorizedException unauthorized:
                        statusCode = StatusCode.Unauthenticated;
                        detail = exception.Message;
                        break;

                    case ForbiddenException forbidden:
                        statusCode = StatusCode.PermissionDenied;
                        detail = exception.Message;
                        break;

                    default:
                        statusCode = StatusCode.Internal;
                        detail = _env.IsDevelopment() ? exception.Message : "An unexpected error occurred. Please try again later.";
                        break;
                }

                var traceId = Activity.Current?.Id ?? context.GetHttpContext().TraceIdentifier;

                if (statusCode == StatusCode.Internal)
                {
                    _logger.LogError(exception, "Unhandled gRPC exception for {Method}. TraceId: {TraceId}",
                        context.Method, traceId);
                }
                else
                {
                    _logger.LogWarning(exception, "Handled gRPC exception ({Status}) for {Method}. TraceId: {TraceId}",
                        statusCode, context.Method, traceId);
                }

                var trailers = new Metadata
                {
                    { "traceId", traceId }
                };

                if (exception is ValidationException ve && ve.Errors.Count > 0)
                {
                    trailers.Add("validation-errors", System.Text.Json.JsonSerializer.Serialize(ve.Errors));
                }

                throw new RpcException(new Status(statusCode, detail), trailers);
            }
        }
    }
}
