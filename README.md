# WareFlow - Sistema de Gestión de Inventarios

API RESTful para gestión de inventarios y órdenes desarrollada con .NET 10.

## 🚀 Tecnologías
- .NET 10
- Entity Framework Core
- SQL Server LocalDB  
- JWT Authentication
- Swagger

## 📦 Endpoints Principales

### Autenticación
- `POST /api/Auth/register` - Registrar usuario
- `POST /api/Auth/login` - Iniciar sesión

### Productos
- `GET /api/Products` - Listar productos
- `POST /api/Products` - Crear producto (Auth)
- `PUT /api/Products/{id}` - Actualizar producto (Auth)
- `DELETE /api/Products/{id}` - Eliminar producto (Auth)

### Órdenes
- `GET /api/Orders` - Listar órdenes (Auth)
- `POST /api/Orders` - Crear orden (Auth)

## 🔧 Cómo ejecutar

```bash
cd backend/WareFlow.API
dotnet run

Luego abre: http://localhost:5276/swagger

