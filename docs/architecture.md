# Arquitectura — WareFlow

Documentación técnica de la arquitectura de WareFlow: visión general del sistema,
capas del backend, flujo de autenticación y modelo de datos.

**Índice**

- [Visión general](#visión-general)
- [Capas del backend](#capas-del-backend)
- [Flujo de autenticación](#flujo-de-autenticación)
- [Modelo de datos](#modelo-de-datos)

---

## Visión general

```mermaid
graph LR
    U["Usuario<br/>(navegador)"] -->|"HTTPS"| F["Frontend SPA<br/>React 19 + Vite 8<br/>Vercel / nginx :80"]
    F -->|"/api/v1/* (JSON + Bearer JWT)"| B["Backend API<br/>ASP.NET Core 10<br/>Azure App Service / Kestrel :8080"]
    B -->|"EF Core 9"| D[("SQL Server<br/>LocalDB (dev)<br/>Azure SQL (prod)")]
    B -.->|"access token 15 min<br/>+ refresh token 7 días"| F

    subgraph DEVLOCAL["Entorno local — docker compose up --build"]
        L1["frontend :3000<br/>(nginx proxy /api/v1/)"]
        L2["backend :8080<br/>(healthcheck /api/v1/Health)"]
        L3["sqlserver :1433"]
        L1 --> L2 --> L3
    end

    subgraph CI["GitHub Actions — ci.yml"]
        J1["job: Backend<br/>(dotnet build + dotnet test)"]
        J2["job: Frontend<br/>(pnpm install + pnpm build)"]
    end
    CI -.->|"push / pull request a main"| F
    CI -.->|"push / pull request a main"| B
```

- El frontend consume **un solo origen** de datos: la API REST versionada en `/api/v1/`.
- En Docker, nginx sirve la SPA y hace de proxy inverso de `/api/v1/` hacia `backend:8080`,
  por lo que no hay CORS entre contenedores (`VITE_API_URL=/api/v1`).
- En despliegue, el frontend (Vercel) y la API (Azure App Service) son orígenes
  distintos: CORS se configura con `Cors:AllowedOrigins` (ver `appsettings.Production.json`).
- La autorización viaja en el header `Authorization: Bearer <access token>` y se
  resuelve con los claims de rol incluidos en el JWT, sin consultar la base de datos.

---

## Capas del backend

Clean Architecture simplificada en 3 capas + proyecto de pruebas (ver
[ADR 001](adr/001-clean-architecture.md)). La flecha indica la dirección de la
referencia entre proyectos.

```mermaid
graph TD
    API["WareFlow.API<br/>Controllers · Program.cs · appsettings<br/>Services (RefreshTokenService) · Data (RoleSeeder)"]
    CORE["WareFlow.Core<br/>Models (User, Product, Order, OrderItem,<br/>InventoryMovement, RefreshToken) · Enums"]
    INFRA["WareFlow.Infrastructure<br/>ApplicationDbContext · Migrations (3)"]
    TESTS["WareFlow.Tests<br/>xUnit + WebApplicationFactory<br/>42 tests de integración (EF Core InMemory)"]

    API --> INFRA
    API --> CORE
    INFRA --> CORE
    TESTS -.->|"WebApplicationFactory&lt;Program&gt;"| API
    TESTS -.->|"DbContext InMemory"| INFRA
```

| Capa | Responsabilidad | No debe contener |
|------|-----------------|------------------|
| `WareFlow.API` | HTTP, autenticación, CORS, Swagger, arranque y migraciones | Reglas de negocio complejas |
| `WareFlow.Core` | Entidades, enums y DTOs del dominio | Referencias a EF Core, ASP.NET o Identity |
| `WareFlow.Infrastructure` | DbContext, mapeo fluent y migraciones | Lógica de negocio |

## Flujo de autenticación

JWT de vida corta + refresh token rotativo (ver [ADR 002](adr/002-jwt-refresh-tokens.md)):

```mermaid
sequenceDiagram
    autonumber
    participant U as Usuario (React)
    participant A as AuthController
    participant R as RefreshTokenService
    participant DB as SQL Server

    U->>A: POST /api/v1/Auth/login { email, password }
    A->>DB: SignInManager.CheckPasswordSignInAsync
    DB-->>A: Succeeded
    A->>A: GenerateJwtToken (15 min, claims de rol)
    A->>R: GenerateAsync(userId)
    R->>R: 64 bytes aleatorios + SHA256 (hash)
    R->>DB: INSERT RefreshTokens (hash, ExpiresAt = +7 días)
    A-->>U: { accessToken, refreshToken, role, expiresIn }

    Note over U,DB: El access token expira a los 15 minutos

    U->>A: GET /api/v1/Products (Bearer expirado)
    A-->>U: 401 Unauthorized
    Note over U: interceptor de Axios: encola la petición y pide refresh
    U->>A: POST /api/v1/Auth/refresh { refreshToken }
    A->>R: ValidateAsync(token) → hash + IsActive (no revocado, no expirado)
    R-->>A: RefreshToken con User incluido
    A->>R: RotateAsync(token) → RevokedAt = now, ReplacedByToken = hash(nuevo)
    R->>DB: UPDATE token usado + INSERT token nuevo
    A-->>U: { accessToken, refreshToken } rotados
    U->>A: reintenta la petición original (ya con el token nuevo)

    U->>A: POST /api/v1/Auth/logout (Bearer)
    A->>R: RevokeAllForUserAsync(userId)
    R->>DB: UPDATE RefreshTokens SET RevokedAt = now (todos los activos)
    A-->>U: 200 { message: "Sesión cerrada correctamente" }
```

Puntos clave:

- En la base de datos sólo se guarda el **hash SHA256** del refresh token; el valor
  en claro existe únicamente en la respuesta al cliente (login y refresh).
- Cada refresh **rota** el token: el usado se revoca y se enlaza al nuevo con
  `ReplacedByToken`, así que un token filtrado sirve una sola vez.
- `logout` revoca todos los tokens activos del usuario: es una revocación real en
  servidor, no sólo un borrado del token en `localStorage`.
- **Roles:** el primer usuario registrado en una base vacía recibe `Admin`; el resto
  entra como `Vendedor` (el `RoleSeeder` del arranque repara además a usuarios sin rol,
  asignando `Admin` al más antiguo y `Vendedor` al resto).

---

## Modelo de datos

Modelo de dominio (6 entidades, mapeadas a 5 tablas propias más `AspNetUsers`) y sus
relaciones. Las tablas de ASP.NET Core Identity (7) se listan en las notas finales.

```mermaid
erDiagram
    USER ||--o{ ORDER : "crea"
    USER ||--o{ REFRESH_TOKEN : "posee"
    USER ||--o{ INVENTORY_MOVEMENT : "registra"
    ORDER ||--|{ ORDER_ITEM : "contiene"
    ORDER ||--o{ INVENTORY_MOVEMENT : "origina"
    PRODUCT ||--o{ ORDER_ITEM : "se vende en"
    PRODUCT ||--o{ INVENTORY_MOVEMENT : "afecta"

    USER {
        string Id PK "GUID (Identity)"
        string Email
        string FirstName
        string LastName
        bool IsActive
        datetime CreatedAt
    }
    ORDER {
        int Id PK
        string OrderNumber "único"
        string UserId FK
        datetime OrderDate
        int Status "OrderStatus 1..6"
        decimal TotalAmount
        string Notes
    }
    ORDER_ITEM {
        int Id PK
        int OrderId FK
        int ProductId FK
        int Quantity
        decimal UnitPrice "precio al momento de la venta"
    }
    PRODUCT {
        int Id PK
        string SKU "único, mayúsculas"
        string Name
        string Description
        decimal Price
        int Stock
        int MinimumStock
        string Category
        bool IsActive "soft delete"
    }
    INVENTORY_MOVEMENT {
        int Id PK
        int ProductId FK
        int Type "MovementType 1..4"
        int Quantity
        string Reason
        string UserId FK
        int OrderId FK "opcional"
    }
    REFRESH_TOKEN {
        int Id PK
        string Token "hash SHA256 (Base64)"
        string UserId FK
        datetime ExpiresAt
        datetime CreatedAt
        datetime RevokedAt "null = activo"
        string ReplacedByToken "hash del sucesor"
    }
```

Enums usados por el modelo:

| Enum | Valores |
|------|---------|
| `OrderStatus` | `Pending`, `Processing`, `Shipped`, `Delivered`, `Cancelled`, `Returned` |
| `MovementType` | `Purchase`, `Sale`, `Return`, `Adjustment` |

Notas de implementación:

- `Price`, `UnitPrice` y `TotalAmount` se mapean como `decimal(18,2)`
  (`HasPrecision(18, 2)` en `OnModelCreating`).
- `OrderItem.Subtotal` (`Quantity × UnitPrice`) es una propiedad calculada: no es columna.
- `Product` usa **soft delete** (`IsActive`): los listados y KPIs filtran los activos.
- `InventoryMovement` se crea automáticamente al procesar una orden (trazabilidad de stock).
- Tablas propias del dominio (5): `Products`, `Orders`, `OrderItems`,
  `InventoryMovements` y `RefreshTokens`.
- Tablas de ASP.NET Core Identity (7): `AspNetUsers` (incluye los campos propios
  `FirstName`, `LastName`, `CreatedAt`, `IsActive`), `AspNetRoles`, `AspNetUserRoles`,
  `AspNetUserClaims`, `AspNetRoleClaims`, `AspNetUserLogins`, `AspNetUserTokens`.
- Con `__EFMigrationsHistory`, la base queda en **13 tablas**.

