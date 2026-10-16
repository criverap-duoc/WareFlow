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
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Forzar UTC en todos los DateTime al leer/escribir en SQL Server
            var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableDateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : v.Value.ToUniversalTime()) : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                        property.SetValueConverter(dateTimeConverter);
                    else if (property.ClrType == typeof(DateTime?))
                        property.SetValueConverter(nullableDateTimeConverter);
                }
            }

            // Configurar Order
            builder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.UserId);

                entity.Property(o => o.TotalAmount)
                    .HasPrecision(18, 2); // 18 dígitos totales, 2 decimales

                // Desglose de IVA (Fase 3): neto + IVA = total con IVA incluido.
                entity.Property(o => o.NetAmount)
                    .HasPrecision(18, 2);

                entity.Property(o => o.TaxAmount)
                    .HasPrecision(18, 2);

                entity.Property(o => o.OrderNumber)
                    .HasMaxLength(50)
                    .IsRequired();

                // Fase IA.2: score de anomalía (0-100, opcional hasta el primer cálculo).
                entity.Property(o => o.AnomalyScore)
                    .HasPrecision(18, 2);
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

            // Configurar RefreshToken (Fase B.3a)
            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasOne(rt => rt.User)
                    .WithMany()
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // El índice único evita duplicados y acelera la búsqueda por hash.
                entity.HasIndex(rt => rt.Token)
                    .IsUnique();

                entity.Property(rt => rt.Token)
                    .HasMaxLength(128)
                    .IsRequired();

                entity.Property(rt => rt.ReplacedByToken)
                    .HasMaxLength(128);
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
