using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WareFlow.Core.Models;

namespace WareFlow.API.Data;

/// <summary>
/// Siembra de roles base de WareFlow (Admin, Vendedor, Bodeguero).
///
/// La creación de roles se serializa con un candado estático: varios hosts pueden
/// arrancar en paralelo dentro del mismo proceso (tests de integración con base
/// InMemory compartida, réplicas en Docker). Sin el candado todos verían
/// "el rol no existe" antes del primer INSERT y se crearían roles duplicados;
/// las consultas internas de Identity buscan el rol por nombre normalizado con
/// SingleOrDefault, por lo que los duplicados rompen el login y el registro.
/// </summary>
public static class RoleSeeder
{
    /// <summary>Roles soportados por la aplicación.</summary>
    public static readonly string[] DefaultRoles = { "Admin", "Vendedor", "Bodeguero" };

    private static readonly SemaphoreSlim SeedLock = new(1, 1);

    /// <summary>
    /// Crea los roles base si no existen y, si la base ya tenía usuarios sin rol
    /// (instalación previa a la Fase B.1), asigna Admin al usuario más antiguo.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        await SeedLock.WaitAsync();
        try
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<User>>();

            foreach (var role in DefaultRoles)
                await EnsureRoleExistsAsync(roleManager, role);

            var firstUser = await userManager.Users.OrderBy(u => u.CreatedAt).FirstOrDefaultAsync();
            if (firstUser != null && !(await userManager.GetRolesAsync(firstUser)).Any())
                await userManager.AddToRoleAsync(firstUser, "Admin");
        }
        finally
        {
            SeedLock.Release();
        }
    }

    /// <summary>Garantiza la existencia de un rol (lo usan los tests de integración).</summary>
    public static async Task EnsureRoleExistsAsync(IServiceProvider services, string roleName)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await SeedLock.WaitAsync();
        try
        {
            await EnsureRoleExistsAsync(roleManager, roleName);
        }
        finally
        {
            SeedLock.Release();
        }
    }

    private static async Task EnsureRoleExistsAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
            return;

        try
        {
            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded && !await roleManager.RoleExistsAsync(roleName))
            {
                throw new InvalidOperationException(
                    $"No se pudo crear el rol '{roleName}': " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        catch (DbUpdateException)
        {
            // Otra instancia del proceso creó el rol al mismo tiempo (Docker/réplicas).
            // Solo se propaga si el rol realmente sigue sin existir.
            if (!await roleManager.RoleExistsAsync(roleName))
                throw;
        }
    }
}
