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
    /// Crea los roles base si no existen y asigna rol a todos los usuarios que no
    /// tengan ninguno (instalación previa a la Fase B.1 o usuarios creados fuera
    /// del endpoint de registro): el más antiguo recibe Admin y el resto Vendedor.
    /// Así ningún usuario queda huérfano (sin permisos) tras el arranque.
    /// </summary>
    /// <returns>Cantidad de usuarios a los que se les asignó rol automáticamente.</returns>
    public static async Task<int> SeedAsync(IServiceProvider services)
    {
        await SeedLock.WaitAsync();
        try
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<User>>();

            foreach (var role in DefaultRoles)
                await EnsureRoleExistsAsync(roleManager, role);

            var users = await userManager.Users.OrderBy(u => u.CreatedAt).ToListAsync();
            var firstUserId = users.FirstOrDefault()?.Id;
            var assigned = 0;

            foreach (var user in users)
            {
                if ((await userManager.GetRolesAsync(user)).Any())
                    continue;

                var roleToAssign = user.Id == firstUserId ? "Admin" : "Vendedor";
                await userManager.AddToRoleAsync(user, roleToAssign);
                assigned++;
            }

            return assigned;
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
