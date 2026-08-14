using Grpc.Net.Client.Configuration;
namespace Ecommerce.API.Grpc;
public static class GrpcClientRegistrationExtensions
{
 public static IServiceCollection AddInternalGrpcClients(this IServiceCollection services, IConfiguration configuration)
 {
  var address=configuration["Grpc:Services:Address"] ?? "https://localhost:5001"; var apiKey=configuration["Grpc:Authentication:ApiKey"] ?? string.Empty;
  var retry=new MethodConfig { Names={MethodName.Default}, RetryPolicy=new RetryPolicy { MaxAttempts=3, InitialBackoff=TimeSpan.FromMilliseconds(100), MaxBackoff=TimeSpan.FromSeconds(1), BackoffMultiplier=2, RetryableStatusCodes={Grpc.Core.StatusCode.Unavailable} } };
  Configure<Ecommerce.Grpc.Contracts.Catalog.V1.CatalogService.CatalogServiceClient>(services,address,apiKey,retry); Configure<Ecommerce.Grpc.Contracts.Order.V1.OrderService.OrderServiceClient>(services,address,apiKey,retry);
  Configure<Ecommerce.Grpc.Contracts.Inventory.V1.InventoryService.InventoryServiceClient>(services,address,apiKey,null); Configure<Ecommerce.Grpc.Contracts.Payment.V1.PaymentService.PaymentServiceClient>(services,address,apiKey,null); return services;
 }
 private static void Configure<T>(IServiceCollection services,string address,string apiKey,MethodConfig? retry) where T:Grpc.Core.ClientBase<T>
 {
  services.AddGrpcClient<T>(o => { o.Address=new Uri(address); o.CallOptionsActions.Add(c => c.CallOptions=c.CallOptions.WithDeadline(DateTime.UtcNow.AddSeconds(5)).WithHeaders(new Grpc.Core.Metadata{{"x-service-api-key",apiKey}})); }).ConfigureChannel(o => { if(retry is not null)o.ServiceConfig=new ServiceConfig{MethodConfigs={retry}}; });
 }
}
