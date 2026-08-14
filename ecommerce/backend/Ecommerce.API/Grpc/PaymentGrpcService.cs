using System.Collections.Concurrent;
using Ecommerce.Grpc.Contracts.Payment.V1;
using Grpc.Core;
namespace Ecommerce.API.Grpc;
public sealed class PaymentGrpcService : PaymentService.PaymentServiceBase
{
 private static readonly ConcurrentDictionary<Guid, PaymentResponse> Authorizations = new();
 public override Task<PaymentResponse> AuthorizePayment(AuthorizePaymentRequest request, ServerCallContext context)
 {
  if (!Guid.TryParse(request.PaymentId, out var paymentId) || !Guid.TryParse(request.OrderId, out _) || !decimal.TryParse(request.Amount, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount <= 0 || string.IsNullOrWhiteSpace(request.Currency) || string.IsNullOrWhiteSpace(request.PaymentToken)) throw new GrpcContractException(StatusCode.InvalidArgument, "Los datos del pago no son válidos.");
  var response = Authorizations.GetOrAdd(paymentId, id => new PaymentResponse { PaymentId=id.ToString(), Status="AUTHORIZED", AuthorizationCode=$"DEV-{id:N}"[..16] });
  return Task.FromResult(response);
 }
}
