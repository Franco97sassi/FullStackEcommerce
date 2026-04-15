using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class Order : BaseEntity
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Confirmed";
    public decimal TotalAmount { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
