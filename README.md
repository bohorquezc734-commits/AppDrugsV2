# 🏥 AppDrugsV2 — API REST & Backend

API RESTful empresarial para la gestión, control de inventario y dispensación de medicamentos en sedes y EPS. Construida bajo los principios de **Clean Architecture**, **CQRS** y **Domain-Driven Design (DDD)** con **.NET 8**.

---

## 🚀 Tecnologías y Arquitectura

- **Framework:** .NET 8 Web API
- **Arquitectura:** Clean Architecture (Domain, Application, Infrastructure, API)
- **Patrones:** CQRS con **MediatR**, Repository Pattern, Entity Framework Core 8
- **Base de Datos:** SQL Server
- **Seguridad:** JWT Bearer Tokens con Refresh Token Rotativo, RBAC (Admin, Pharmacist, User), Rate Limiting
- **Observabilidad:** Logging estructurado con **Serilog** (Consola + Archivos con rotación diaria)
- **Auditoría:** Interceptor automatizado (`AuditableEntityInterceptor`) con resolución semántica de Enums y campos
- **DevOps:** Dockerfile Multi-Stage, GitHub Actions CI/CD pipeline

---

## 🏗️ Estructura del Proyecto

```text
AppDrugsV2/
├── AppDrugsV2.Domain/         # Entidades del negocio, Enums e Interfaces base
├── AppDrugsV2.Application/    # Casos de Uso, CQRS (Commands/Queries), DTOs y Mappings
├── AppDrugsV2.Infrastructure/ # EF Core DbContext, Interceptores, Serilog, Repositorios
├── AppDrugsV2.Api/            # Controllers, Middlewares, Rate Limiting, Program.cs
└── AppDrugsV2.UnitTests/      # Pruebas unitarias de la capa de aplicación
🔒 Módulos y Endpoints PrincipalesControladorRuta BaseDescripciónAuthController/api/AuthAutenticación, Refresh Token, Gestión de Usuarios y RolesDrugsController/api/DrugsCatálogo maestro de medicamentos (CRUD paginado)InventoriesController/api/InventoriesControl de stock por sede y alertas de stock bajoAppointmentsController/api/AppointmentsGestión de turnos, estados, generación/validación de QRGestoresController/api/GestoresGestión de sedes y puntos farmacéuticos (EPS)AuditLogsController/api/AuditLogsHistorial inmutable de auditoría con paginación server-sideNotificationsController/api/NotificationsNotificaciones del usuario autenticadoReportsController/api/ReportsGeneración y exportación de reportes en PDF y Excel🛡️ Características de ProducciónPaginación Server-Side Generica (PagedResult<T>): Consultas optimizadas con .Skip() y .Take() a nivel de base de datos.Rate Limiting (Protección Anti-Fuerza Bruta):/api/auth/login: Límite de 5 intentos por cada 60 segundos./api/auth/forgot-password: Límite de 3 intentos por cada 300 segundos.Auditoría Inteligente: Registro automático de cambios con nombres traducidos al español, conversión de estados enums a cadenas de texto legibles y tracking por usuario.🛠️ Instalación y Configuración LocalPrerrequisitos.NET 8 SDKSQL Server LocalDB o Instancia de SQL ServerPasosClona el repositorio:Bashgit clone [https://github.com/bohorquezc734-commits/AppDrugsV2.git](https://github.com/bohorquezc734-commits/AppDrugsV2.git)
cd AppDrugsV2
Configura las cadenas de conexión y secretos en AppDrugsV2.Api/appsettings.json:JSON{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AppDrugsDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Secret": "TuClaveSecretaSuperSeguraDeAlMenos32Caracteres!",
    "ExpiryMinutes": 60
  }
}
Aplica las migraciones de Entity Framework Core:Bashdotnet ef database update --project AppDrugsV2.Infrastructure --startup-project AppDrugsV2.Api
Ejecuta la aplicación:Bashdotnet run --project AppDrugsV2.Api
HTTP: http://localhost:5071Swagger UI: http://localhost:5071/swagger🐳 Ejecución con DockerBash# Construir la imagen
docker build -t appdrugs-api .

# Ejecutar el contenedor
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Server=..." \
  -e JwtSettings__Secret="..." \
  appdrugs-api
