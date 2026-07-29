using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ecommerce.Tests;

public sealed class EcommerceApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");

    public EcommerceApiFactory()
    {
        connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<EcommerceDbContext>>();
            services.RemoveAll<EcommerceDbContext>();
            services.AddDbContext<EcommerceDbContext>(options => options.UseSqlite(connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            connection.Dispose();
        }
    }

    public async Task<TestData> ResetAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        var category = new Category { Name = "Integration", Slug = "integration" };
        var product = new Product
        {
            Name = "Integration product",
            Slug = "integration-product",
            Price = 25m,
            Stock = 5,
            Category = category
        };
        var admin = new User
        {
            FullName = "Test Admin",
            Email = "admin@test.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "Admin"
        };

        db.AddRange(category, product, admin);
        await db.SaveChangesAsync();
        return new TestData(category.Id, product.Id);
    }
}

public sealed record TestData(Guid CategoryId, Guid ProductId);
