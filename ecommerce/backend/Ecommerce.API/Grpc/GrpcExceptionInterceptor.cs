using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Ecommerce.API.Grpc;

public sealed class GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (GrpcContractException exception)
        {
            throw new RpcException(new Status(exception.StatusCode, exception.Message));
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            throw new RpcException(new Status(StatusCode.Cancelled, "La operación fue cancelada."));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled gRPC error in {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.Internal, "Ocurrió un error interno."));
        }
    }
}
