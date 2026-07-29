using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ecommerce.API.Contracts;

namespace Ecommerce.Tests;

public sealed class ApiFlowTests(EcommerceApiFactory factory) : IClassFixture<EcommerceApiFactory>
{
	private static readonly ShippingAddressRequest Address = new(
		"Integration Customer", "123 Test Street", "Madrid", "Madrid", "28001", "España", null);

	[Fact]
	public async Task Registration_and_login_return_working_JWTs()
	{
		await factory.ResetAsync();
		using var client = factory.CreateClient();

		var registration = await RegisterAsync(client, "customer@test.local");
		client.DefaultRequestHeaders.Authorization = Bearer(registration);
		Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/cart")).StatusCode);

		var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("customer@test.local", "Customer123!"));
		var login = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
		Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
		Assert.False(string.IsNullOrWhiteSpace(login?.AccessToken));
	}

	[Fact]
	public async Task Protected_routes_enforce_authentication_and_admin_role()
	{
		await factory.ResetAsync();
		using var client = factory.CreateClient();

		Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/cart")).StatusCode);
		var customer = await RegisterAsync(client, "customer@test.local");
		client.DefaultRequestHeaders.Authorization = Bearer(customer);
		Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/admin/products")).StatusCode);

		var admin = await LoginAsync(client, "admin@test.local", "Admin123!");
		client.DefaultRequestHeaders.Authorization = Bearer(admin);
		Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/admin/products")).StatusCode);
	}

	[Fact]
	public async Task Catalog_and_cart_support_add_update_and_remove_flow()
	{
		var data = await factory.ResetAsync();
		using var client = factory.CreateClient();

		var catalog = await client.GetFromJsonAsync<PagedProductsResponse>("/api/catalog/products");
		Assert.Contains(catalog!.Items, item => item.Id == data.ProductId);

		client.DefaultRequestHeaders.Authorization = Bearer(await RegisterAsync(client, "customer@test.local"));
		var added = await (await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest(data.ProductId, 1)))
			.Content.ReadFromJsonAsync<CartResponse>();
		Assert.Equal(1, added!.TotalItems);

		var updated = await (await client.PatchAsJsonAsync($"/api/cart/items/{data.ProductId}", new UpdateCartItemRequest(2)))
			.Content.ReadFromJsonAsync<CartResponse>();
		Assert.Equal(2, updated!.TotalItems);

		Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync($"/api/cart/items/{data.ProductId}")).StatusCode);
		Assert.Empty((await client.GetFromJsonAsync<CartResponse>("/api/cart"))!.Items);
	}

	[Fact]
	public async Task Checkout_succeeds_with_stock_rejects_shortage_and_hides_other_users_order()
	{
		var data = await factory.ResetAsync();
		using var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization = Bearer(await RegisterAsync(client, "first@test.local"));

		var checkout = await client.PostAsJsonAsync("/api/checkout/orders", Order(data.ProductId, 2));
		var order = await checkout.Content.ReadFromJsonAsync<CreateOrderResponse>();
		Assert.Equal(HttpStatusCode.Created, checkout.StatusCode);
		Assert.Equal(50m, order!.Total);

		var shortage = await client.PostAsJsonAsync("/api/checkout/orders", Order(data.ProductId, 4));
		Assert.Equal(HttpStatusCode.BadRequest, shortage.StatusCode);

		client.DefaultRequestHeaders.Authorization = Bearer(await RegisterAsync(client, "second@test.local"));
		Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/checkout/orders/{order.OrderId}")).StatusCode);
	}

	[Fact]
	public async Task Administrator_can_complete_category_CRUD()
	{
		await factory.ResetAsync();
		using var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization = Bearer(await LoginAsync(client, "admin@test.local", "Admin123!"));

		var create = await client.PostAsJsonAsync("/api/admin/categories", new AdminCategoryRequest
		{
			Name = "Accessories",
			Slug = "accessories",
			Description = "Created by integration test"
		});
		var category = await create.Content.ReadFromJsonAsync<AdminCategoryResponse>();
		Assert.Equal(HttpStatusCode.Created, create.StatusCode);

		var update = await client.PutAsJsonAsync($"/api/admin/categories/{category!.Id}", new AdminCategoryRequest
		{
			Name = "Updated accessories",
			Slug = "updated-accessories",
			IsActive = true
		});
		Assert.Equal(HttpStatusCode.OK, update.StatusCode);
		Assert.Contains(await client.GetFromJsonAsync<List<AdminCategoryResponse>>("/api/admin/categories") ?? [], x => x.Id == category.Id);
		Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/admin/categories/{category.Id}")).StatusCode);
	}

	private static CreateOrderRequest Order(Guid productId, int quantity) =>
		new([new CreateOrderItemRequest(productId, quantity)], Address);

	private static AuthenticationHeaderValue Bearer(AuthResponse auth) => new("Bearer", auth.AccessToken);

	private static async Task<AuthResponse> RegisterAsync(HttpClient client, string email)
	{
		var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Integration Customer", email, "Customer123!"));
		response.EnsureSuccessStatusCode();
		return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
	}

	private static async Task<AuthResponse> LoginAsync(HttpClient client, string email, string password)
	{
		var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
		response.EnsureSuccessStatusCode();
		return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
	}
}
