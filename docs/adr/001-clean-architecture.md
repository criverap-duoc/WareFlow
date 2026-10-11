# ADR 001 — Arquitectura en 3 capas (API / Core / Infrastructure)

- **Fecha:** 2026-10-10
- **Estado:** Aceptada
- **Decisores:** Cristóbal Vera (desarrollo completo)

## Contexto

WareFlow es un proyecto de portafolio full-stack desarrollado por una sola
persona. Necesita demostrar separación de responsabilidades (el dominio no debe
conocer detalles de persistencia ni de transporte HTTP) sin caer en
sobre-ingeniería que ralentice la entrega de features reales.

El código original tenía la lógica de negocio, los modelos y el acceso a datos
en un único proyecto ASP.NET Core, lo que dificultaba razonar sobre qué dependía
de qué y hacía imposible reutilizar el dominio en pruebas sin levantar todo el host.

## Decisión

Separar la solución en 3 proyectos con dependencias explícitas:

- `WareFlow.API` → Controllers, Program.cs, appsettings, servicios de aplicación
  (RefreshTokenService, RoleSeeder).
- `WareFlow.Core` → entidades de dominio (`User`, `Product`, `Order`, `OrderItem`,
  `InventoryMovement`, `RefreshToken`), enums y DTOs. No referencia a nadie.
- `WareFlow.Infrastructure` → `ApplicationDbContext`, configuraciones fluent
  (`HasPrecision(18, 2)`) y migraciones EF Core.

Regla de dependencias: `API → Infrastructure → Core` y `API → Core`. El dominio
nunca referencia EF Core, ASP.NET ni Identity.

## Consecuencias

**Positivas**
- El dominio es testeable y legible sin infraestructura.
- Las migraciones y el DbContext quedan aislados: cambiar el motor de base de
  datos toca una sola capa.
- Los tests de integración pueden reemplazar el DbContext por EF Core InMemory
  (`CustomWebApplicationFactory`) sin tocar los controllers.

**Negativas / costos**
- Más proyectos que mantener y referencias que actualizar al mover tipos.
- DTOs duplicados respecto al modelo en algunos endpoints (aceptado por ahora).

## Alternativas consideradas

- **Monolito MVC en un solo proyecto:** más rápido al inicio, pero mezcla dominio
  con infraestructura y no demuestra separación de capas.
- **Capas como carpetas dentro de un proyecto:** mantiene la organización visual
  pero no impone la dirección de dependencias (nada impide un `using` indebido).
- **Clean Architecture completa (Domain / Application / Persistence + MediatR, CQRS):**
  correcta para equipos grandes, pero introduce ceremonia que no se justifica en
  un proyecto individual de portafolio.
