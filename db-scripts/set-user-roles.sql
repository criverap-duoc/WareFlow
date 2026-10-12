-- set-user-role.sql
-- Cambia el rol de un usuario de WareFlow
-- Uso: reemplaza @email y @roleName, ejecuta en VS Code con Ctrl+Shift+E

DECLARE @email NVARCHAR(256) = 'test@test.com';   -- ← CAMBIA ESTO
DECLARE @roleName NVARCHAR(256) = 'Admin';             -- ← CAMBIA ESTO (Admin | Vendedor | Bodeguero)

DECLARE @userId NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = @email);
DECLARE @roleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = @roleName);

IF @userId IS NULL
BEGIN
  PRINT 'ERROR: usuario no encontrado';
  RETURN;
END

IF @roleId IS NULL
BEGIN
  PRINT 'ERROR: rol no encontrado';
  RETURN;
END

DELETE FROM AspNetUserRoles WHERE UserId = @userId;
INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@userId, @roleId);

PRINT 'Rol actualizado correctamente';
SELECT u.Email, r.Name AS NuevoRol
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = @email;