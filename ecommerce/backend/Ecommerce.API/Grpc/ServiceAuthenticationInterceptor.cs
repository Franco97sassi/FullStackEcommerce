using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Options;

namespace Ecommerce.API.Grpc;

public sealed class ServiceAuthenticationOptions
{
    public const string SectionName = "Grpc:Authentication";
    public string ApiKey { get; set; } = string.Empty;
}

public sealed class ServiceAuthenticationInterceptor(
    IOptions<ServiceAuthenticationOptions> options,
    IWebHostEnvironment environment) : Interceptor
{
    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var configuredKey = options.Value.ApiKey;
        var suppliedKey = context.RequestHeaders.GetValue("x-service-api-key");
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing"))
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Service authentication is not configured."));
            }
        }
        else if (!System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                     System.Text.Encoding.UTF8.GetBytes(configuredKey),
                     System.Text.Encoding.UTF8.GetBytes(suppliedKey ?? string.Empty)))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Credenciales de servicio inválidas."));
        }

        return continuation(request, context);
    }
}
