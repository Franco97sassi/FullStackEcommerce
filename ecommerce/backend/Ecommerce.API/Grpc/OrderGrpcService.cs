using Ecommerce.Grpc.Contracts.Order.V1;
using Ecommerce.Infrastructure.Persistence;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
namespace Ecommerce.API.Grpc;
public sealed class OrderGrpcService(EcommerceDbContext db) : OrderService.OrderServiceBase
{
 public override async Task<OrderResponse> GetOrder(GetOrderRequest request, ServerCallContext context)
 {
  if (!Guid.TryParse(request.OrderId, out var id)) throw new GrpcContractException(StatusCode.InvalidArgument, "order_id no es válido.");
  var order = await db.Orders.AsNoTracking().Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, context.CancellationToken) ?? throw new GrpcContractException(StatusCode.NotFound, "Pedido no encontrado.");
  var response = new OrderResponse { OrderId=order.Id.ToString(), UserId=order.UserId.ToString(), Status=order.Status, TotalAmount=order.TotalAmount.ToString(System.Globalization.CultureInfo.InvariantCulture), CreatedAtUnixMs=new DateTimeOffset(DateTime.SpecifyKind(order.CreatedAtUtc, DateTimeKind.Utc)).ToUnixTimeMilliseconds() };
  response.Items.AddRange(order.Items.Select(x => new OrderLine { ProductId=x.ProductId.ToString(), Quantity=x.Quantity, UnitPrice=x.UnitPrice.ToString(System.Globalization.CultureInfo.InvariantCulture), LineTotal=x.LineTotal.ToString(System.Globalization.CultureInfo.InvariantCulture) }));
  return response;
 }
}
