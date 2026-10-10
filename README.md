# 📦 WareFlow — Sistema de Gestión de Inventarios y Órdenes

[![CI](https://github.com/criverap-duoc/WareFlow/actions/workflows/ci.yml/badge.svg)](https://github.com/criverap-duoc/WareFlow/actions/workflows/ci.yml)
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
- **Prod (planificado):** Azure SQL Database

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
npm install
npm run dev
```
- App: `http://localhost:5173`

### 6. Credenciales de prueba
```
Email: cri.verap@duocuc.cl
Password: Iceland12.
```
> Si no funciona, registra un usuario nuevo desde `/login`. Las contraseñas deben tener mínimo 6 caracteres, una mayúscula, un número y un carácter especial.

---

## 📚 Endpoints de la API

### Autenticación
| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `POST` | `/api/Auth/register` | ❌ | Registrar usuario |
| `POST` | `/api/Auth/login` | ❌ | Iniciar sesión (retorna JWT) |

### Productos
| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `GET` | `/api/Products` | ❌ | Listar productos activos |
| `GET` | `/api/Products/{id}` | ❌ | Obtener producto por ID |
| `POST` | `/api/Products` | ✅ | Crear producto |
| `PUT` | `/api/Products/{id}` | ✅ | Actualizar producto |
| `DELETE` | `/api/Products/{id}` | ✅ | Eliminar producto (soft delete) |

### Órdenes
| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `GET` | `/api/Orders` | ✅ | Listar órdenes del usuario |
| `GET` | `/api/Orders/{id}` | ✅ | Detalle de orden |
| `POST` | `/api/Orders` | ✅ | Crear orden (valida stock) |
| `PUT` | `/api/Orders/{id}/status` | ✅ | Actualizar estado de orden |

### Health
| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `GET` | `/api/Health` | ❌ | Health check de la API |

---

## 📸 Capturas de pantalla

> *(Sección en construcción — se agregarán capturas de: Login, Dashboard de productos con KPIs, Carrito, Historial de órdenes, Swagger.)*

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
| Documentación | 🟡 50% |
| Tests automatizados | ⏳ Pendiente |
| Docker | ⏳ Pendiente |
| CI/CD | ⏳ Pendiente |
| Despliegue en la nube | ⏳ Pendiente |

**Progreso global:** ~82%

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
- [ ] Versionado de API (`/api/v1/`)

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
- [ ] Despliegue en Azure App Service + Azure SQL
- [ ] Frontend en Vercel
- [ ] Video demo (2 min)
- [ ] Diagrama de arquitectura
- [ ] ADRs (Architecture Decision Records)

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