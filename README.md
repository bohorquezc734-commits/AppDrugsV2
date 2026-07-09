# 🏥 AppDrugsV2 API

API REST para gestión de medicamentos/farmacia construida con **.NET 8** siguiendo los principios de **Clean Architecture** y **CQRS**.

---

## 🛠️ Tecnologías

| Tecnología | Versión | Uso |
|------------|---------|-----|
| .NET | 8.0 | Framework base |
| ASP.NET Core | 8.0 | Web API |
| Entity Framework Core | 8.0 | ORM - Acceso a datos |
| SQL Server | - | Base de datos |
| MediatR | 12.0 | CQRS - Desacoplamiento de handlers |
| FluentValidation | 11.0 | Validación de requests |
| BCrypt.Net-Next | 4.2 | Hash de contraseñas |
| JWT Bearer | 8.0 | Autenticación con tokens |
| Serilog | 10.0 | Logging estructurado |
| Swagger/OpenAPI | 6.5 | Documentación de API |

---

## 📦 Funcionalidades

- ✅ Registro de usuarios con roles (Admin, Pharmacist, User)
- ✅ Autenticación JWT (login con token)
- ✅ CRUD de medicamentos
- ✅ Control de stock y vencimientos
- ✅ Búsqueda y filtros por categoría/receta
- ✅ Paginación de resultados
- ✅ Validaciones con FluentValidation
- ✅ Documentación Swagger

---

## 🏗️ Arquitectura

El proyecto sigue **Clean Architecture** con 4 capas:
