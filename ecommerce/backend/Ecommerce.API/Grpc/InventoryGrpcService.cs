using System.Collections.Concurrent;
using Ecommerce.Grpc.Contracts.Inventory.V1;
using Ecommerce.Infrastructure.Persistence;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Grpc;

public sealed class InventoryReservationStore
{
    public ConcurrentDictionary<Guid, IReadOnlyDictionary<Guid, int>> Reservations { get; } = new();
    public SemaphoreSlim Gate { get; } = new(1, 1);
}

public sealed class InventoryGrpcService(EcommerceDbContext db, InventoryReservationStore store) : InventoryService.InventoryServiceBase
{
    public override async Task<AvailabilityResponse> CheckAvailability(CheckAvailabilityRequest request, ServerCallContext context)
    {
        var lines = Parse(request.Items);
        var stock = await db.Products.AsNoTracking().Where(x => lines.Keys.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Stock, context.CancellationToken);
        var response = new AvailabilityResponse { Available = lines.All(x => stock.GetValueOrDefault(x.Key) >= x.Value) };
        response.Items.AddRange(lines.Select(x => new AvailabilityLine
        {
            ProductId = x.Key.ToString(), RequestedQuantity = x.Value,
            AvailableQuantity = stock.GetValueOrDefault(x.Key), Available = stock.GetValueOrDefault(x.Key) >= x.Value
        }));
        return response;
    }

    public override async Task<ReservationResponse> ReserveInventory(ReserveInventoryRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ReservationId, out var reservationId))
            throw new GrpcContractException(StatusCode.InvalidArgument, "reservation_id no es válido.");
        var lines = Parse(request.Items);
        await store.Gate.WaitAsync(context.CancellationToken);
        try
        {
            if (store.Reservations.ContainsKey(reservationId)) return new ReservationResponse { ReservationId = request.ReservationId, Reserved = true };
            var products = await db.Products.Where(x => lines.Keys.Contains(x.Id)).ToDictionaryAsync(x => x.Id, context.CancellationToken);
            if (lines.Any(x => !products.TryGetValue(x.Key, out var product) || !product.IsActive || product.Stock < x.Value))
                throw new GrpcContractException(StatusCode.FailedPrecondition, "Stock insuficiente para completar la reserva.");
            foreach (var line in lines) products[line.Key].Stock -= line.Value;
            await db.SaveChangesAsync(context.CancellationToken);
            store.Reservations[reservationId] = lines;
            return new ReservationResponse { ReservationId = request.ReservationId, Reserved = true };
        }
        finally { store.Gate.Release(); }
    }

    public override async Task<ReleaseInventoryResponse> ReleaseInventory(ReleaseInventoryRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ReservationId, out var reservationId))
            throw new GrpcContractException(StatusCode.InvalidArgument, "reservation_id no es válido.");
        await store.Gate.WaitAsync(context.CancellationToken);
        try
        {
            if (!store.Reservations.TryRemove(reservationId, out var lines)) return new ReleaseInventoryResponse { ReservationId = request.ReservationId, Released = false };
            var products = await db.Products.Where(x => lines.Keys.Contains(x.Id)).ToDictionaryAsync(x => x.Id, context.CancellationToken);
            foreach (var line in lines.Where(x => products.ContainsKey(x.Key))) products[line.Key].Stock += line.Value;
            await db.SaveChangesAsync(context.CancellationToken);
            return new ReleaseInventoryResponse { ReservationId = request.ReservationId, Released = true };
        }
        finally { store.Gate.Release(); }
    }

    private static IReadOnlyDictionary<Guid, int> Parse(IEnumerable<InventoryLine> items)
    {
        var lines = new Dictionary<Guid, int>();
        foreach (var item in items)
        {
            if (!Guid.TryParse(item.ProductId, out var id) || item.Quantity <= 0)
                throw new GrpcContractException(StatusCode.InvalidArgument, "Cada línea requiere product_id válido y quantity mayor a cero.");
            lines[id] = lines.GetValueOrDefault(id) + item.Quantity;
        }
        if (lines.Count == 0) throw new GrpcContractException(StatusCode.InvalidArgument, "Se requiere al menos una línea.");
        return lines;
    }
}
