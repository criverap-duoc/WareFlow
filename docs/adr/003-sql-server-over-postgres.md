# ADR 003 — SQL Server en vez de PostgreSQL

- **Fecha:** 2026-10-10
- **Estado:** Aceptada
- **Decisores:** Cristóbal Vera (desarrollo completo)

## Contexto

El proyecto necesitaba un motor relacional para 13 tablas (6 de dominio + 7 de
ASP.NET Core Identity) con acceso concurrente desde la API. El entorno de
desarrollo es Windows y el despliegue planificado es Azure.

Los criterios de evaluación fueron: integración con el stack .NET, costo de
operación en la nube, disponibilidad de tier gratuito y ecosistema de
herramientas de migración.

## Decisión

Usar **SQL Server** con `Microsoft.EntityFrameworkCore.SqlServer`:

- **Desarrollo:** SQL Server LocalDB (`(localdb)\MSSQLLocalDB`), ya instalado
  con el SDK y sin contenedor ni servicio adicional.
- **Producción (planificado):** Azure SQL Database, con proyecto de despliegue
  en Azure App Service para mantener ambos en el mismo proveedor y accesibles
  por red privada.
- **Docker:** imagen `mssql/server` en `docker-compose.yml` con healthcheck, de
  modo que `docker compose up --build` levanta el entorno completo.

El esquema se versiona con migraciones EF Core (3 aplicadas) y se aplica al
arranque con `Database.Migrate()` cuando el proveedor es relacional.

## Consecuencias

**Positivas**
- `UseSqlServer` + `dotnet ef` es la ruta mejor soportada de EF Core: menos
  fricción para generar y aplicar migraciones.
- LocalDB permite desarrollar sin Docker ni conexión a internet.
- Credenciales del tier gratuito de Azure SQL para el portafolio y coherencia
  con el resto del stack Azure (App Service).
- Identity funciona sin ajustes: los `HasPrecision`, índices y tipos de Identity
  están alineados con SQL Server.

**Negativas / costos**
- Licencia propietaria y costo si se supera el tier gratuito.
- Dependencia del ecosistema Microsoft (menor portabilidad).
- LocalDB es exclusivo de Windows: en Linux/macOS se necesita el contenedor.
- El mapeo de tipos (`decimal(18,2)`, `datetime2`, `nvarchar`) exige cuidado al
  migrar a otro motor.

## Alternativas consideradas

- **PostgreSQL:** mejor portabilidad, gratis y con gran ecosistema; se descartó
  porque el host es Azure y la integración/identity con SQL Server reduce el
  trabajo de configuración y soporte.
- **MySQL:** viable y económico, pero con menos afinidad con el tooling de
  EF Core en .NET y sin ventajas claras frente a las otras dos opciones.
- **SQLite:** excelente para desarrollo y tests rápidos, pero sin concurrencia
  de escritura real ni historial de tipos/decimales adecuado para producción.
- **EF Core InMemory:** sólo como doble de pruebas (es lo que usan los 42 tests
  de integración), no como base de datos de aplicación.
