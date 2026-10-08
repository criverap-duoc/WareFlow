# Automation Workflows — WareFlow

Ejecutar todos los comandos desde Git Bash o PowerShell, según se indique.

## Niveles de verificación

### Nivel 1 — Tras CUALQUIER edit (rápido, obligatorio)
- Backend: `dotnet build WareFlow.API` (desde `backend/`)
- Frontend: verificar visualmente en el navegador (`pnpm dev` corriendo)
- No ejecutar la suite completa de tests

### Nivel 2 — Al cerrar una feature (medio)
- `dotnet build` (solución completa)
- `dotnet test` (si hay tests para el módulo)
- Verificar que la feature funciona end-to-end en el navegador

### Nivel 3 — Antes de commit/tag (completo)
- Nivel 2 +
- `git status --short` (listar cambios)
- `git diff --cached --stat` (revisar lo que se va a commitear)
- Checklist de integridad (ver abajo)

## Comandos disponibles

### "project health check"
Reportar:
1. `git status --short`
2. `git log --oneline -5`
3. `dotnet build --nologo -v q` (desde `backend/`) → debe terminar sin errores
4. `dotnet test --nologo -v q` (si existen tests) → contar tests
5. `ls backend/WareFlow.Infrastructure/Migrations/` (última migración)

### "clean commit"
1. `git add -A`
2. `git diff --cached --stat`
3. Generar mensaje Conventional Commit y **mostrarlo al usuario para aprobación**
4. Solo tras aprobación: `git commit -m "<mensaje>"`
5. No hacer push automático — el usuario decide

### "tag and release"
1. Confirmar formato `vX.Y.Z`
2. `git tag -a vX.Y.Z -m "VX.Y.Z: <descripción>"`
3. `git push origin main`
4. `git push origin vX.Y.Z`

### "cerrar feature"
1. `dotnet build` (desde `backend/`) → debe salir 0 errores
2. `dotnet test` → todos pasan (si existen)
3. Verificar feature en navegador (login → feature → logout)
4. `git status --short`
5. Reportar al usuario y esperar aprobación antes de commitear

### "verificación post-edit backend"
Tras editar cualquier `.cs`:
1. `dotnet build WareFlow.API --nologo -v q`
2. Si hay error de sintaxis → corregir ANTES de continuar
3. No ejecutar tests completos salvo en Nivel 2

### "verificación pre-commit"
1. `dotnet build` (solución completa)
2. `dotnet test` (si existen)
3. `git status --short`
4. Reportar resumen y esperar confirmación

## Checklist de integridad (Nivel 3)

Antes de cada commit importante, verificar:

- [ ] ¿Los cambios en `Product` requieren migración? → crear migración
- [ ] ¿Los cambios en `Order` afectan la lógica de stock? → verificar `OrdersController`
- [ ] ¿Se agregaron endpoints nuevos? → probar en Swagger
- [ ] ¿Se modificó el frontend? → probar en navegador (login + flujo completo)
- [ ] ¿Se tocó `Program.cs`? → verificar Swagger + auth siguen funcionando
- [ ] ¿Se agregó una dependencia NuGet/npm (adaptar a pnpm, NO se usa npm.)? → verificar que el build sigue OK

## Reglas anti-desperdicio de tokens

1. **No ejecutar `dotnet test` tras cada edit.** Solo en Nivel 2 o 3.
2. **No rebuildear toda la solución tras un edit de 1 línea.** Usar
   `dotnet build WareFlow.API` (solo el proyecto afectado).
3. **No pegar output completo de build.** Reportar solo errores/warnings.
4. **No ejecutar seed SQL salvo que sea necesario.** Los datos persisten
   en LocalDB.
5. **Máximo 2 edits por archivo por turno.** Para 3+ cambios, hacer un
   solo edit grande o esperar al siguiente turno.