using Ecommerce.Domain.Entities;

namespace Ecommerce.Tests;

public class DomainEntitiesTests
{
    [Fact]
    public void Product_HasActiveDefaultAndGeneratedId()
    {
        var product = new Product();

        Assert.True(product.IsActive);
        Assert.NotEqual(Guid.Empty, product.Id);
    }

    [Fact]
    public void User_HasCustomerRoleAndActiveByDefault()
    {
        var user = new User();

        Assert.Equal("Customer", user.Role);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Order_HasConfirmedStatusAndUtcTimestampByDefault()
    {
        var before = DateTime.UtcNow.AddMinutes(-1);
        var order = new Order();
        var after = DateTime.UtcNow.AddMinutes(1);

        Assert.Equal("Confirmed", order.Status);
        Assert.InRange(order.CreatedAtUtc, before, after);
    }
}
