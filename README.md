# 📦 WareFlow — Sistema de Gestión de Inventarios y Órdenes

[![CI](https://github.com/criverap-duoc/WareFlow/actions/workflows/ci.yml/badge.svg)](https://github.com/criverap-duoc/WareFlow/actions/workflows/ci.yml)
[![Live Demo](https://img.shields.io/badge/demo-live-success)](https://wareflow-vercel.vercel.app)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=white)](https://react.dev/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-9.0.3-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-CC2927?logo=microsoftsqlserver&logoColor=white)](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb)
[![JWT](https://img.shields.io/badge/Auth-JWT-000000?logo=jsonwebtokens&logoColor=white)](https://jwt.io/)
[![Swagger](https://img.shields.io/badge/API%20Docs-Swagger-85EA2D?logo=swagger&logoColor=black)](https://swagger.io/)
[![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](#-licencia)

> API RESTful + SPA en React para gestión de inventarios, productos y órdenes con lógica de negocio real: control de stock, máquina de estados de órdenes, movimientos de inventario y dashboard de KPIs.

---

## 🎯 ¿Qué es WareFlow?

WareFlow es un **ERP ligero** desarrollado como proyecto de portafolio full-stack. Simula el flujo operativo real de una PYME: autenticación de usuarios, catálogo de productos con filtros avanzados, carrito de compras, creación de órdenes con validación de stock y trazabilidad completa mediante movimientos de inventario.

**No es un CRUD de tutorial.** Incluye lógica de negocio transaccional, máquina de estados, validaciones de negocio y un frontend funcional que consume la API end-to-end.

### 🧩 Problema que resuelve
Las PYMEs suelen gestionar inventario y ventas en planillas Excel desconectadas, lo que provoca sobreventa, quiebres de stock y falta de trazabilidad. WareFlow centraliza productos, stock y órdenes en una API transaccional con interfaz web, aplicando reglas de negocio que evitan inconsistencias (por ejemplo: no se puede vender más stock del disponible).

---

## ✨ Características Principales

### 🔐 Autenticación y Autorización
- Registro y login de usuarios con **ASP.NET Core Identity**
- Autenticación basada en **JWT Bearer** (expiración configurable, 7 días por defecto)
- Claims enriquecidos: `sub`, `email`, `jti`, `givenname`, `surname`
- Rutas protegidas en frontend con redirección automática al login

### 📦 Gestión de Productos (CRUD completo)
- Crear, listar, ver, actualizar y eliminar (soft delete) productos
- Campos: nombre, descripción, SKU único, precio, stock, stock mínimo, categoría, imagen
- Validaciones: SKU único, precio > 0, stock ≥ 0
- SKU normalizado a mayúsculas automáticamente

### 🔎 Filtros y Ordenamiento Avanzado
- Búsqueda por nombre, SKU o categoría
- Filtro por categoría
- Filtro por rango de precio (`$0–100K`, `$100K–500K`, `$500K–1M`, `$1M+`)
- Filtro por estado de stock (bajo, sin stock, con stock)
- Ordenamiento por nombre, precio, stock o categoría (asc/desc)

### 📊 Dashboard de KPIs en tiempo real
- Total de productos activos
- Valor total del inventario (Σ precio × stock)
- Productos con stock bajo (stock ≤ stock mínimo)
- Productos sin stock
- Cantidad de categorías
- Precio promedio del catálogo

### 🛒 Carrito de Compras
- Estado global con **React Context API**
- Persistencia en `localStorage` entre sesiones
- Agregar, eliminar y actualizar cantidades (respetando stock máximo)
- Cálculo automático de subtotales y total
- Badge con contador en el navbar

### 📋 Sistema de Órdenes con Lógica de Inventario
- Creación de órdenes con **validación transaccional de stock**
- Descuento automático de inventario al confirmar la orden
- Registro de **movimientos de inventario** (`InventoryMovement`) por cada transacción
- Numeración automática: `ORD-YYYYMMDD-XXXX`
- Máquina de estados:
  `Pending → Processing → Shipped → Delivered` (con soporte para `Cancelled` y `Returned`)
- Transiciones validadas: no se puede saltar de `Pending` a `Delivered`
- Cancelación restaura el stock automáticamente
- Visibilidad por usuario: agentes ven sus órdenes; el historial completo es consultable

### 🖼️ Imágenes inteligentes por categoría
- Detección automática por nombre/categoría: laptops, monitores, teclados, mouses, audífonos, CPU, GPU, SSD, RAM, smartphones, tablets
- Fallback a imagen genérica con manejo de errores en frontend

### 🎨 Frontend React
- Login/registro conectado a la API
- CRUD completo de productos con formulario validado
- Carrito y checkout
- Página de historial de órdenes con detalle expandible
- Navbar unificado con estado activo y badge del carrito

---

## 🏗️ Arquitectura

### Backend — Clean Architecture simplificada

```
backend/
├── WareFlow.API/            → Capa de presentación
│   ├── Controllers/         → AuthController, ProductsController,
│   │                          OrdersController, HealthController
│   ├── Program.cs           → DI, JWT, CORS, Swagger
│   └── appsettings.json     → Configuración (DB, JWT, CORS)
│
├── WareFlow.Core/           → Capa de dominio
│   ├── Models/              → User, Product, Order, OrderItem,
│   │                          InventoryMovement
│   └── Enums/               → OrderStatus, MovementType
│
└── WareFlow.Infrastructure/ → Capa de persistencia
    ├── Data/                → ApplicationDbContext
    └── Migrations/          → Migraciones EF Core
```

### Frontend — SPA por features

```
frontend/src/
├── components/              → Navbar
├── pages/                   → Login, Products, Cart, Orders
├── context/                 → AuthContext, CartContext
├── services/                → api.js (Axios + interceptores JWT)
├── App.jsx                  → Rutas protegidas
└── main.jsx                 → Entry point
```

### Modelo de datos

```
User ──1:N──> Order ──1:N──> OrderItem ──N:1──> Product
                  │                                │
                  └──1:N──> InventoryMovement <──1:N
```

- **User → Order**: un usuario puede tener múltiples órdenes
- **Order → OrderItem**: una orden contiene uno o más items
- **OrderItem → Product**: cada item referencia un producto
- **Product → InventoryMovement**: cada producto tiene historial de movimientos
- **Order → InventoryMovement**: trazabilidad de movimientos por orden

---

## 🗺️ Diagrama de arquitectura

WareFlow usa una **Clean Architecture simplificada en 3 capas**: `WareFlow.API`
(HTTP, autenticación, Swagger), `WareFlow.Core` (entidades y enums del dominio,
sin dependencias) y `WareFlow.Infrastructure` (EF Core, migraciones). El frontend
es una SPA por features (pages / components / context / services) que consume
únicamente la API versionada en `/api/v1/`.

Ver diagrama completo en [docs/architecture.md](docs/architecture.md) (visión
general, capas, flujo de autenticación y modelo de datos).

## 🧭 Decisiones técnicas

Las decisiones de arquitectura se documentan como ADRs (Architecture Decision Records):

| ADR | Decisión |
|-----|----------|
| [ADR 001](docs/adr/001-clean-architecture.md) | Clean Architecture en 3 capas (API / Core / Infrastructure) |
| [ADR 002](docs/adr/002-jwt-refresh-tokens.md) | JWT de 15 min + refresh token de 7 días con rotación |
| [ADR 003](docs/adr/003-sql-server-over-postgres.md) | SQL Server / Azure SQL en vez de PostgreSQL |
| [ADR 004](docs/adr/004-tailwind-shadcn.md) | Tailwind CSS 4 + shadcn/ui en el frontend |

Ver [docs/adr/](docs/adr/) para las decisiones arquitectónicas.

---

## 🛠️ Stack Tecnológico

### Backend
| Tecnología | Versión | Uso |
|------------|---------|-----|
| .NET | 10.0 | Framework principal |
| ASP.NET Core | 10.0 | Web API |
| Entity Framework Core | 9.0.3 | ORM |
| SQL Server LocalDB | 17.0 | Base de datos (dev) |
| ASP.NET Core Identity | 9.0.3 | Gestión de usuarios |
| JWT Bearer | 9.0.3 | Autenticación |
| Swashbuckle | 7.2.0 | Swagger UI |
| Serilog | 9.0.0 | Logging (instalado) |

### Frontend
| Tecnología | Versión | Uso |
|------------|---------|-----|
| React | 19 | UI Library |
| Vite | 8.0.3 | Build tool |
| React Router | 7.13.2 | Enrutamiento |
| Axios | Latest | Cliente HTTP |
| Context API | Nativo | Estado global |

### Base de datos
- **Dev:** SQL Server LocalDB (`MSSQLLocalDB`)
- **Prod:** Azure SQL Database (Brazil South, Free Tier)

---

## 🌐 Demo en vivo

| Servicio | URL |
|----------|-----|
| Frontend (Vercel) | [wareflow-vercel.vercel.app](https://wareflow-vercel.vercel.app) |
| Backend API (Azure App Service) | https://wareflow-api-csvera-eweqd3c9ctanbvcj.brazilsouth-01.azurewebsites.net |
| Swagger | https://wareflow-api-csvera-eweqd3c9ctanbvcj.brazilsouth-01.azurewebsites.net/swagger |

> La base de datos está en plan **Azure SQL Free Tier (serverless)**: entra en
> *auto-pause* tras un periodo de inactividad, por lo que la primera petición puede
> tardar unos segundos mientras despierta (el backend tiene reintentos automáticos
> configurados para ese caso).
>
> Usuario demo: `cri.verap@duocuc.cl` / `Iceland12.`

---

## 🚀 Quick Start

### Opcion 1: Docker (recomendado)

Requiere Docker Desktop instalado.

```bash
git clone https://github.com/criverap-duoc/WareFlow.git
cd WareFlow
docker compose up --build
```

Servicios:
- Frontend: http://localhost:3000
- Backend API: http://localhost:8080
- Swagger: http://localhost:8080/swagger
- SQL Server: localhost:1433 (sa / WareFlow2026!)

Para detener:

```bash
docker compose down
```

Para eliminar datos de la base:

```bash
docker compose down -v
```

### Opcion 2: Local (desarrollo)

### Prerrequisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/)
- [SQL Server Express LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb)
- [dotnet-ef CLI](https://learn.microsoft.com/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`

### 1. Clonar el repositorio
```bash
git clone https://github.com/criverap-duoc/WareFlow.git
cd WareFlow
```

### 2. Configurar la base de datos
```bash
cd backend/WareFlow.API
dotnet ef database update --project ../WareFlow.Infrastructure --startup-project .
```

### 3. (Opcional) Poblar con datos de prueba
```bash
# Desde la raíz del proyecto
sqlcmd -S "(localdb)\MSSQLLocalDB" -d "WareFlowDB" -i "seed-massive-products.sql"
```
Esto inserta **60+ productos** en 8 categorías (laptops, monitores, teclados, mouses, audífonos, componentes, smartphones, tablets).

### 4. Levantar el backend
```bash
cd backend/WareFlow.API
dotnet run
```
- API: `http://localhost:5276`
- Swagger: `http://localhost:5276/swagger`

### 5. Levantar el frontend
```bash
# En otra terminal
cd frontend
pnpm install
pnpm dev
```
- App: `http://localhost:5173`

### 6. Credenciales de prueba
```
Email: test@test.com
Password: Test123.
```
> Si no funciona, registra un usuario nuevo desde `/login`. Las contraseñas deben tener mínimo 6 caracteres, una mayúscula, un número y un carácter especial.

---

## 📚 Endpoints de la API

### Autenticación
| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `POST` | `/api/v1/Auth/register` | ❌ | Registrar usuario |
| `POST` | `/api/v1/Auth/login` | ❌ | Iniciar sesión (retorna JWT) |

### Productos
| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `GET` | `/api/v1/Products` | ❌ | Listar productos activos |
| `GET` | `/api/v1/Products/{id}` | ❌ | Obtener producto por ID |
| `POST` | `/api/v1/Products` | ✅ | Crear producto |
| `PUT` | `/api/v1/Products/{id}` | ✅ | Actualizar producto |
| `DELETE` | `/api/v1/Products/{id}` | ✅ | Eliminar producto (soft delete) |

### Órdenes
| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `GET` | `/api/v1/Orders` | ✅ | Listar órdenes del usuario |
| `GET` | `/api/v1/Orders/{id}` | ✅ | Detalle de orden |
| `POST` | `/api/v1/Orders` | ✅ | Crear orden (valida stock) |
| `PUT` | `/api/v1/Orders/{id}/status` | ✅ | Actualizar estado de orden |

### Health
| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `GET` | `/api/v1/Health` | ❌ | Health check de la API |

---

## 📸 Capturas

> *(GIFs pendientes de grabación — se agregan en la Fase 5.)*

| Flujo | GIF |
|-------|-----|
| Login + dark mode | [`docs/gifs/01-login.gif`](docs/gifs/01-login.gif) |
| Productos con filtros y paginación | [`docs/gifs/02-products.gif`](docs/gifs/02-products.gif) |
| Carrito y checkout | [`docs/gifs/03-cart.gif`](docs/gifs/03-cart.gif) |
| Historial de órdenes | [`docs/gifs/04-orders.gif`](docs/gifs/04-orders.gif) |

Una vez grabados, cada GIF se embebe con `![descripción](docs/gifs/0X-nombre.gif)`.

---

## 🧪 Estado del proyecto

| Módulo | Estado |
|--------|--------|
| Backend (.NET 10 + EF Core) | ✅ 95% |
| Autenticación JWT | ✅ 100% |
| CRUD de productos | ✅ 100% |
| Sistema de órdenes + inventario | ✅ 100% |
| Frontend React (login, productos, carrito, órdenes) | ✅ 85% |
| Base de datos + seed | ✅ 95% |
| Documentación | ✅ 90% |
| Tests automatizados | ✅ 42 tests |
| Docker | ✅ |
| CI/CD | ✅ |
| Despliegue en la nube | ✅ Azure + Vercel |
| Demo pública | ✅ https://wareflow-vercel.vercel.app |

**Progreso global:** ~95%

---

## 🗺️ Roadmap

### Fase 1 — Producto terminado
- [x] Autenticación JWT
- [x] CRUD de productos
- [x] Sistema de órdenes con validación de stock
- [x] Carrito de compras
- [x] Dashboard de KPIs
- [x] Filtros y ordenamiento
- [ ] Roles de usuario (Admin / Vendedor / Bodeguero)
- [ ] Paginación server-side
- [ ] Refresh tokens
- [x] Versionado de API (`/api/v1/`)

### Fase 2 — Ingeniería de producción
- [ ] Tests unitarios e integración (xUnit + WebApplicationFactory)
- [ ] Dockerfile multi-stage + docker-compose
- [ ] CI/CD con GitHub Actions
- [ ] Serilog configurado (consola + archivo)
- [ ] Health Checks (`/health`, `/health/ready`)
- [ ] Rate Limiting en endpoints de auth

### Fase 3 — IA aplicada
- [ ] Predicción de demanda de stock (ML.NET)
- [ ] Detección de anomalías en órdenes (OpenAI API)

### Fase 4 — Contexto chileno
- [ ] Cálculo automático de IVA (19%) y formato CLP
- [ ] Simulación de DTE (SII)
- [ ] Simulación de flujo de pago Webpay

### Fase 5 — Narrativa y despliegue
- [x] Despliegue en Azure App Service + Azure SQL
- [x] Frontend en Vercel
- [ ] Video demo (2 min)
- [x] Diagrama de arquitectura
- [x] ADRs (Architecture Decision Records)

---

## ⚠️ Limitaciones del tier gratuito

Este proyecto usa exclusivamente servicios gratuitos:

- **Backend (Azure App Service F1):** tiene 60 minutos de CPU por día
  y se duerme tras 20 minutos de inactividad. El primer request después
  de dormir puede tardar 20-60 segundos (cold start). Si ves un error
  503 o de CORS, espera un minuto y reintenta.

- **Base de datos (Azure SQL Serverless):** se pausa tras 60 minutos
  de inactividad. El primer query después de pausar puede tardar
  10-30 segundos.

- **Frontend (Vercel Hobby):** sin límites relevantes para este uso.

Para producción real, se recomienda subir a un tier sin cold start
(App Service B1, Azure SQL Provisioned).

---

## 🤝 Contribuciones

Este es un proyecto de portafolio personal, pero sugerencias y feedback son bienvenidos vía issues.

---

## 📄 Licencia

MIT — ver [LICENSE](LICENSE) para más detalles.

---

## 👨‍💻 Autor

**Cristóbal Vera** — Analista Programador Computacional (Duoc UC) · Químico Industrial (PUCV)

- GitHub: [@criverap-duoc](https://github.com/criverap-duoc)
- Repositorio: [github.com/criverap-duoc/WareFlow](https://github.com/criverap-duoc/WareFlow)

---

<p align="center">
  <sub>Desarrollado con .NET 10 + React 19 · Proyecto de portafolio full-stack</sub>
</p>