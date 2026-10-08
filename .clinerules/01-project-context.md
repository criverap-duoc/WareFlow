# Project Context: WareFlow

## Stack
- Backend: .NET 10 (ASP.NET Core), EF Core 9, SQL Server LocalDB (dev) / Azure SQL (prod planificado)
- Frontend: React 19 + Vite 8, JavaScript (NO TypeScript por ahora), Context API, Axios
- Auth: JWT Bearer (7 días de expiración), Identity para usuarios
- Testing: xUnit + WebApplicationFactory (por implementar)
- IA planificada: ML.NET (predicción demanda) + OpenAI API (anomalías)

## Arquitectura
- Clean Architecture simplificada en 3 capas:
  - `WareFlow.API` → Controllers, Program.cs, appsettings
  - `WareFlow.Core` → Models, Enums, DTOs, Interfaces
  - `WareFlow.Infrastructure` → ApplicationDbContext, Migrations
- Frontend por features: pages/, components/, context/, services/
- API versionado: `/api/v1/` (migración pendiente desde `/api/`)

## Decisiones clave
- Permisos vía Identity Roles (Admin, Vendedor, Bodeguero) — pendiente
- Soft delete en Product (IsActive) — implementado
- Máquina de estados en Order (Pending → Processing → Shipped → Delivered)
- Movimientos de inventario automáticos en cada orden (InventoryMovement)
- Imágenes por URL externa + fallback por categoría (getDefaultImage)
- Paginación server-side — pendiente, alta prioridad

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
- Usuario demo: cri.verap@duocuc.cl / Iceland12.