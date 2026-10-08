using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using WareFlow.Infrastructure.Data;

namespace WareFlow.Tests.Integration;

/// <summary>
/// Factory de integración que reemplaza el DbContext real de SQL Server por
/// EF Core InMemory, aislando las pruebas de la base de datos de desarrollo.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string DatabaseName = "WareFlowTestDb";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Quitar el registro de SQL Server: tanto las DbContextOptions como la
            // configuración del proveedor (EF Core 9 registra IDbContextOptionsConfiguration<T>).
            var descriptorsToRemove = services
                .Where(descriptor =>
                    descriptor.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                    (descriptor.ServiceType.IsGenericType &&
                     descriptor.ServiceType.GetGenericTypeDefinition().Name == "IDbContextOptionsConfiguration`1"))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Registrar el proveedor InMemory con nombre fijo para las pruebas.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(DatabaseName));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Asegurar que la base InMemory esté creada antes de ejecutar los tests.
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();

        return host;
    }
}
