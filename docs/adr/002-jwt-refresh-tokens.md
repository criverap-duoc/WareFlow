# ADR 002 — JWT de 15 minutos + refresh token de 7 días con rotación

- **Fecha:** 2026-10-10
- **Estado:** Aceptada
- **Decisores:** Cristóbal Vera (desarrollo completo)

## Contexto

La autenticación inicial emitía un único JWT con 7 días de vigencia. Un token
robado era válido una semana completa y no existía forma de revocarlo: el estado
de sesión vivía sólo en el cliente, sin registro en el servidor.

La API es consumida por una SPA React y por Swagger, y en la fase de despliegue
(Azure App Service + Vercel) el frontend y el backend son dominios distintos.

## Decisión

Dos artefactos por sesión:

- **Access token JWT de 15 minutos** (`Jwt:AccessExpireMinutes`), stateless, con
  claims de rol para autorizar cada request sin consultar la base de datos.
- **Refresh token de 7 días** (`Jwt:RefreshExpireDays`) persistido en la tabla
  `RefreshTokens`, guardado **hasheado con SHA256** y rotado en cada uso:
  `RotateAsync` revoca el token usado (`RevokedAt`) y anota su reemplazo en
  `ReplacedByToken`, de modo que un token filtrado sirve una sola vez.

El cliente (interceptor de Axios en `frontend/src/services/api.js`) renueva el
access token ante un `401`, con cola de peticiones para no disparar refresh
concurrentes. `POST /api/v1/Auth/logout` revoca todos los tokens activos del usuario.

## Consecuencias

**Positivas**
- Ventana de exposición de un access token robado: 15 minutos.
- El refresh token es revocable en servidor (logout real) y reutilizable una sola vez.
- En la base de datos nunca se almacena el valor utilizable del token.

**Negativas / costos**
- Un round-trip extra al expirar el access token (cada 15 minutos de uso activo).
- La tabla `RefreshTokens` requiere limpieza periódica de tokens expirados
  (pendiente: job de purga).
- La rotación exige manejar correctamente los 401 concurrentes en el cliente.

## Alternativas consideradas

- **Sesiones clásicas server-side con cookie:** revocación inmediata, pero
  requiere estado de sesión, protección CSRF y complica el consumo desde SPA
  en otro dominio sin configuración adicional de cookies cross-site.
- **JWT único de larga duración (estado anterior):** simple, pero no revocable y
  con una ventana de exposición inaceptable.
- **Refresh token sin rotación:** más simple, pero un token filtrado es
  reutilizable durante 7 días sin forma de detectarlo.
- **Cookies httpOnly para el refresh token:** mitiga XSS, pero obliga a
  compartir dominio/`SameSite` entre Vercel y Azure y a manejar CSRF; se evalúa
  para la fase de despliegue si el frontend y la API quedan en dominios distintos.
