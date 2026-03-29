using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WareFlow.Core.Models;

namespace WareFlow.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurar Order
            builder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.UserId);

                entity.Property(o => o.TotalAmount)
                    .HasPrecision(18, 2); // 18 dígitos totales, 2 decimales

                entity.Property(o => o.OrderNumber)
                    .HasMaxLength(50)
                    .IsRequired();
            });

            // Configurar OrderItem
            builder.Entity<OrderItem>(entity =>
            {
                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId);

                entity.HasOne(oi => oi.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(oi => oi.ProductId);

                entity.Property(oi => oi.UnitPrice)
                    .HasPrecision(18, 2);
            });

            // Configurar Product
            builder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.SKU)
                    .IsUnique();

                entity.Property(p => p.Price)
                    .HasPrecision(18, 2);

                entity.Property(p => p.Name)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(p => p.SKU)
                    .HasMaxLength(50)
                    .IsRequired();
            });

            // Configurar InventoryMovement
            builder.Entity<InventoryMovement>(entity =>
            {
                entity.HasOne(im => im.Product)
                    .WithMany(p => p.InventoryMovements)
                    .HasForeignKey(im => im.ProductId);

                entity.HasOne(im => im.User)
                    .WithMany()
                    .HasForeignKey(im => im.UserId);

                entity.HasOne(im => im.Order)
                    .WithMany()
                    .HasForeignKey(im => im.OrderId);
            });

            // Configurar User
            builder.Entity<User>(entity =>
            {
                entity.Property(u => u.FirstName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(u => u.LastName)
                    .HasMaxLength(100)
                    .IsRequired();
            });
        }
    }
}
