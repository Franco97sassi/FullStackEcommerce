using Grpc.Core;

namespace Ecommerce.API.Grpc;

public sealed class GrpcContractException(StatusCode statusCode, string message) : Exception(message)
{
    public StatusCode StatusCode { get; } = statusCode;
}
