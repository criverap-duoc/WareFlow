# Project Context: WareFlow

## Stack
- Backend: .NET 10 (ASP.NET Core), EF Core 9, SQL Server LocalDB (dev) / Azure SQL (prod planificado)
- Frontend: React 19 + Vite 8, JavaScript (NO TypeScript por ahora), Context API, Axios
- Auth: JWT Bearer (15 min) + refresh tokens (7 días, rotación), Identity para usuarios
- Testing: xUnit + WebApplicationFactory (56 tests)
- IA implementada:
  - Predicción de demanda (estadística sobre InventoryMovement)
  - Detección de anomalías (Z-score estadístico)
  - Búsqueda semántica (Google Gemini gemini-embedding-2, 768 dims)
  - ML.NET 4.0.2 instalado para futuras extensiones

## Arquitectura
- Clean Architecture simplificada en 3 capas:
  - `WareFlow.API` → Controllers, Program.cs, appsettings
  - `WareFlow.Core` → Models, Enums, DTOs, Interfaces
  - `WareFlow.Infrastructure` → ApplicationDbContext, Migrations
- Frontend por features: pages/, components/, context/, services/
- API versionado: `/api/v1/` (implementado)

## Decisiones clave
- Permisos vía Identity Roles (Admin, Vendedor, Bodeguero) — implementado
- Soft delete en Product (IsActive) — implementado
- Máquina de estados en Order (Pending → Processing → Shipped → Delivered)
- Movimientos de inventario automáticos en cada orden (InventoryMovement)
- Imágenes por URL externa + fallback por categoría (getDefaultImage)
- Paginación server-side en Products y Orders — implementada

## Rutas locales
- Backend API: `backend/WareFlow.API/`
- Modelos: `backend/WareFlow.Core/Models/`
- DbContext: `backend/WareFlow.Infrastructure/Data/`
- Migraciones: `backend/WareFlow.Infrastructure/Migrations/`
- Frontend: `frontend/src/`
- Scripts SQL: `backend/seed-*.sql`

## Comandos
- Backend dev: `dotnet run` (desde `backend/WareFlow.API/`)
- Frontend dev: `pnpm dev` (desde `frontend/`)
- Frontend install: `pnpm install` (desde `frontend/`)
- Build: `dotnet build` (desde `backend/`)
- Tests: `dotnet test` (desde `backend/`)
- Migración nueva: `dotnet ef migrations add <Nombre> --project ../WareFlow.Infrastructure --startup-project .`
- Aplicar migración: `dotnet ef database update --project ../WareFlow.Infrastructure --startup-project .`
- Seed productos: `sqlcmd -S "(localdb)\MSSQLLocalDB" -d "WareFlowDB" -i "seed-massive-products.sql"`

## Configuración (secretos)
- Gemini API: requiere `Gemini__ApiKey` en Azure App Service y en
  `appsettings.Development.json` (gitignored)
- `GEMINI_API_KEY` en el `.env` del host para docker-compose
- El repositorio es público: la key NUNCA se commitea (`appsettings.json`
  mantiene `"ApiKey": ""`)

## Package manager (regla estricta)
- Frontend: SIEMPRE `pnpm`. NUNCA `npm` ni `yarn`.
- Lockfile del proyecto: `pnpm-lock.yaml` (nunca `package-lock.json`)
- Instalar paquete: `pnpm add <paquete>`
- Instalar devDependency: `pnpm add -D <paquete>`
- Si aparece `package-lock.json` o `yarn.lock` → reportar, no eliminar sin aprobación

## URLs de desarrollo
- Backend: http://localhost:5276
- Swagger: http://localhost:5276/swagger
- Frontend: http://localhost:5173
- Usuario demo: test@test.com / Test123.