using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Ecommerce.API.Contracts;
using Ecommerce.Application.Events;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Tests;

public sealed class KafkaOutboxIntegrationTests(EcommerceApiFactory factory) : IClassFixture<EcommerceApiFactory>
{
    [Fact]
    public async Task Checkout_persists_order_and_event_in_the_same_database()
    {
        var data = await factory.ResetAsync();
        using var client = factory.CreateClient();
        var registrationResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest("Kafka Customer", "kafka@test.local", "Customer123!"));
        var authentication = await registrationResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authentication!.AccessToken);

        var request = new CreateOrderRequest(
            [new CreateOrderItemRequest(data.ProductId, 2)],
            new ShippingAddressRequest(
                "Kafka Customer",
                "123 Event Street",
                "Madrid",
                "Madrid",
                "28001",
                "Spain",
                null));
        var response = await client.PostAsJsonAsync("/api/checkout/orders", request);
        var order = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
        var outboxMessage = await dbContext.OutboxMessages.SingleAsync();
        var orderCreated = JsonSerializer.Deserialize<OrderCreatedEvent>(
            outboxMessage.Payload,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Equal("ecommerce.orders.created.v1", outboxMessage.Topic);
        Assert.Equal(order!.OrderId.ToString(), outboxMessage.MessageKey);
        Assert.Equal(nameof(OrderCreatedEvent), outboxMessage.EventType);
        Assert.Equal(order.OrderId, orderCreated!.OrderId);
        Assert.Equal(2, orderCreated.Items.Single().Quantity);
        Assert.Null(outboxMessage.ProcessedAtUtc);
    }
}
