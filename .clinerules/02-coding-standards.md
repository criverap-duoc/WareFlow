# Coding Standards — WareFlow

## Backend (.NET / C#)

### Naming
- PascalCase: clases, métodos, propiedades, DTOs
- camelCase: parámetros, variables locales
- Prefijo `I` para interfaces (`IProductService`)
- DTOs con sufijo: `CreateProductDto`, `UpdateProductDto`, `ProductResponseDto`

### Estructura
- 1 controller por entidad, en `WareFlow.API/Controllers/`
- DTOs inline en el mismo archivo del controller (etapa actual). Cuando
  un controller supere los 300 LOC, extraer DTOs a `WareFlow.Core/DTOs/`.
- Lógica de negocio en servicios si supera 3 pasos. Actualmente inline
  en controllers por simplicidad.

### EF Core
- `Include` + `ThenInclude` para navegación en listados
- `.Select()` para proyectar solo lo necesario (evitar over-fetching)
- Transacciones explícitas en operaciones multi-tabla:
  `using var transaction = await _context.Database.BeginTransactionAsync();`
- Decimales siempre con `HasPrecision(18, 2)` en `OnModelCreating`
- `[Authorize]` a nivel de controller; `[AllowAnonymous]` en endpoints públicos

### Errores y respuestas
- Formato de error: `{ "message": "...", "error": "..." }` (etapa actual)
- Cuando se migre a Problem Details estándar, cambiar en Program.cs
- No exponer stack traces en producción (`app.Environment.IsDevelopment()`)

## Frontend (React / JavaScript)

### Naming
- PascalCase: componentes (`Products.jsx`, `Navbar.jsx`)
- camelCase: funciones, variables, hooks
- Archivos de página: PascalCase (`Login.jsx`, `Cart.jsx`)
- Archivos de servicio: camelCase (`api.js`)

### Componentes
- 1 componente por archivo
- Props desestructuradas en la firma
- Estilos inline con objeto `styles` al final del archivo (etapa actual)
- Cuando un componente supere 400 LOC, extraer estilos a CSS module

### Estado
- Context API para estado global (Auth, Cart)
- `useState` local para estado de UI
- Persistencia: `localStorage` para token + carrito
- Nunca guardar contraseñas ni datos sensibles en localStorage

### API Client (`src/services/api.js`)
- Instancia única de Axios
- Interceptor de request: adjunta `Authorization: Bearer <token>`
- Interceptor de response: manejar 401 (logout automático) — pendiente
- Servicios por dominio: `authService`, `productService`, `orderService`

### Rutas
- Todas las rutas en `App.jsx`
- Rutas protegidas con `<Navigate to="/login" />` si no hay sesión
- Navbar común en todas las páginas autenticadas (componente `Navbar`)

## Git

### Commits (Conventional Commits)
- Formato: `<type>(<scope>): <descripción>`
- Types: `feat`, `fix`, `docs`, `test`, `refactor`, `chore`, `perf`, `ci`
- Scopes: `api`, `core`, `infra`, `frontend`, `db`, `ci`, `docs`
- Ejemplos:
  - `feat(api): add pagination to products endpoint`
  - `fix(frontend): correct price display in product card`
  - `test(api): add integration tests for orders controller`

### Ramas
- `main`: estable, siempre compilable
- `feat/<nombre>`: features nuevas
- `fix/<nombre>`: correcciones
- Etapa actual: commits directos a `main` (proyecto personal)