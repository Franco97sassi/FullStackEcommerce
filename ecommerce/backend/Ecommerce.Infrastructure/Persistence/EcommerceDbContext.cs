using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Ecommerce.Infrastructure.Persistence;

public class EcommerceDbContext : DbContext
{
    public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.Slug)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.Slug).IsUnique();
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.PasswordHash)
                .IsRequired();

            entity.Property(x => x.Role)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasOne(x => x.Cart)
              .WithOne(x => x.User)
              .HasForeignKey<Cart>(x => x.UserId)
              .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Slug)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Description)
                .HasMaxLength(2000);

            entity.Property(x => x.Price)
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            entity.Property(x => x.Stock)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.Slug).IsUnique();

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Status)
                .IsRequired();

            entity.Property(x => x.TotalAmount)
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            entity.Property(x => x.ShippingFullName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.ShippingAddressLine1)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(x => x.ShippingCity)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(x => x.ShippingState)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(x => x.ShippingPostalCode)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(x => x.ShippingCountry)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(x => x.ShippingPhone)
                .HasMaxLength(30);

            entity.HasOne(x => x.User)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.CreatedAtUtc);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UnitPrice)
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            entity.Property(x => x.LineTotal)
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            entity.HasOne(x => x.Order)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Product)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<Cart>(entity =>
        {
            entity.ToTable("carts");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UpdatedAtUtc)
                .IsRequired();

            entity.HasIndex(x => x.UserId)
                .IsUnique();
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("cart_items");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Quantity)
                .IsRequired();

            entity.HasOne(x => x.Cart)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Product)
                .WithMany(x => x.CartItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.CartId, x.ProductId })
                .IsUnique();
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages");
            entity.HasKey(message => message.Id);
            entity.Property(message => message.Topic).IsRequired().HasMaxLength(255);
            entity.Property(message => message.MessageKey).IsRequired().HasMaxLength(255);
            entity.Property(message => message.EventType).IsRequired().HasMaxLength(255);
            entity.Property(message => message.Payload).IsRequired().HasColumnType("text");
            entity.Property(message => message.OccurredAtUtc).IsRequired();
            entity.Property(message => message.LastError).HasMaxLength(2000);
            entity.HasIndex(message => new { message.ProcessedAtUtc, message.NextAttemptAtUtc });
        });


    }
}
