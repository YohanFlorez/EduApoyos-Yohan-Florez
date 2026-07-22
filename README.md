# EduApoyos — Sistema de gestión de solicitudes de apoyo económico

Aplicación full stack (.NET 8 + Angular 18) para la gestión de solicitudes de beca/crédito/
subsidio de una institución de educación superior. Este repositorio cubre **RF-01 a RF-05**
del enunciado, tanto en el backend como en el frontend.

```
EduApoyos/
  src/          → Backend (.NET 8) — ver detalle más abajo
  tests/        → Pruebas xUnit + Moq (Application, Infrastructure, Integración)
  sql/          → Ejercicios SQL requeridos
  frontend/     → Frontend Angular 18 — ver frontend/README.md
  docker-compose.yml → Levanta SQL Server + API + Frontend juntos
```

**Para el detalle específico del frontend (vistas, guards, sistema de diseño, decisiones),
ver [`frontend/README.md`](./frontend/README.md).** Lo que sigue documenta el backend.


## Stack

| Componente | Tecnología |
|---|---|
| Backend | .NET 8 — ASP.NET Core Web API |
| ORM | Entity Framework Core 8 (Code First) |
| Base de datos | SQL Server |
| Autenticación | ASP.NET Core Identity + JWT Bearer |
| Validación | FluentValidation |
| Documentación | Swagger / OpenAPI (Swashbuckle) |
| Pruebas | xUnit + Moq + FluentAssertions |

## Arquitectura

Clean Architecture en 4 capas, de adentro hacia afuera:

```
src/
  EduApoyos.Domain          → Entidades, enums, interfaces de repositorio, reglas de negocio.
                               Sin dependencias externas (ni siquiera EF Core).
  EduApoyos.Application     → Casos de uso (services), DTOs, validadores FluentValidation,
                               interfaces hacia infraestructura (puertos).
  EduApoyos.Infrastructure  → EF Core, ASP.NET Core Identity, repositorios, JWT, migraciones, DI.
  EduApoyos.Api             → Controladores, middleware, Swagger, Program.cs.
tests/
  EduApoyos.Application.Tests    → xUnit + Moq sobre los casos de uso (capa Application).
  EduApoyos.Infrastructure.Tests → xUnit + Moq sobre repositorios y servicios de infraestructura.
  EduApoyos.IntegrationTests     → Pruebas de extremo a extremo con WebApplicationFactory.
sql/
  consultas_requeridas.sql  → Los 3 ejercicios SQL pedidos en la prueba.
```

La regla de dependencia se respeta en una sola dirección: `Api → Infrastructure → Application → Domain`.
`Domain` no conoce a nadie; `Application` solo conoce interfaces (`IUnitOfWork`, `IJwtTokenGenerator`,
`IUsuarioLookupService`, etc.), nunca implementaciones concretas de EF Core o Identity.

### Decisión: Usuario vs. ASP.NET Core Identity

El modelo de datos del enunciado pide una entidad `Usuario` con `PasswordHash`, `Rol`, etc.
En vez de duplicar esa tabla, `ApplicationUser : IdentityUser<Guid>` (en Infrastructure) **es**
esa entidad: Identity ya resuelve el hash de contraseña, lockout y tokens; se le agregaron los
campos `NombreCompleto`, `Rol` y `FechaRegistro`. Las tablas de Identity se renombraron a español
(`Usuarios`, `Roles`, etc.) para mantener consistencia con el modelo de datos.

## Patrones de diseño aplicados

1. **Repository + Unit of Work** (`IEstudianteRepository`, `ISolicitudRepository`, `IUnitOfWork`):
   aísla a `Application` de EF Core, facilita mockear en pruebas unitarias y centraliza el
   `SaveChanges` en una sola transacción por request.
2. **Strategy** (`IEstadoSolicitudStrategy` / `FlujoEstandarEstadoSolicitudStrategy`): la regla de
   qué transiciones de estado son válidas (`Pendiente → EnRevision → Aprobada/Rechazada`) vive
   aislada de la entidad `SolicitudApoyo`. Si mañana se necesita un flujo distinto (por ejemplo,
   un estado intermedio "EnComité" para créditos grandes), se agrega una nueva estrategia sin
   tocar la entidad ni los casos de uso — cumple abierto/cerrado (OCP).

Adicionalmente se aplicó un **Aggregate Root** (DDD-lite): `SolicitudApoyo` es dueña de su
`HistorialEstado` y es la única que puede generarlo, a través del método `CambiarEstado(...)`,
así que es imposible que exista un cambio de estado sin su registro de trazabilidad correspondiente.

## Base de datos y migraciones

Las migraciones de EF Core (Code First) están generadas y versionadas en
`src/EduApoyos.Infrastructure/Migrations/`, con historial progresivo que refleja la evolución
real del modelo durante el desarrollo:

| Migración | Qué agrega |
|---|---|
| `InitialCreate` | Esquema base: `Usuarios` (Identity), `Estudiantes`, `SolicitudApoyo`, `HistorialEstado`. |
| `AgregarFKUsuarioIdEnEstudiante` | Relación `Estudiante.UsuarioId` → `Usuarios`, para vincular el portal de autogestión con su perfil académico. |
| `AjusteHistorialEstadoId` | Ajuste de tipo/generación de clave en `HistorialEstado`. |
| `AgregarActivoAEstudiante` | Columna `Activo` (baja lógica) para soportar `DesactivarAsync`/`ActivarAsync` sin borrar historial de solicitudes asociado. |

No se requiere ejecutar `dotnet ef migrations add` para levantar el proyecto: al iniciar la API
(tanto en Docker como en local) se ejecuta `Database.Migrate()`, que aplica automáticamente
cualquier migración pendiente contra la base de datos configurada.

## Seguridad

- JWT Bearer con expiración configurable (`Jwt:ExpiracionMinutos` en `appsettings.json`).
- Autorización por rol con `[Authorize(Roles = "Asesor")]` / `"Estudiante"`.
- Autorización por **recurso**: un estudiante que consulta `GET /api/solicitudes/{id}` o
  `GET /api/estudiantes/{id}/solicitudes` solo puede ver las suyas — validado en
  `SolicitudService`, no solo en el controlador, para que la regla no se pueda saltar
  agregando otro endpoint.
- Contraseñas hasheadas por ASP.NET Core Identity (nunca texto plano).
- Secretos (cadena de conexión, `Jwt:Key`) fuera de `appsettings.json` en `appsettings.Development.json`
  (ignorado por Docker/prod) o variables de entorno; en producción real deberían ir en Azure Key Vault
  o `dotnet user-secrets`.
- Errores estandarizados con `ProblemDetails` (RFC 7807) vía `ExceptionHandlingMiddleware`, sin
  exponer stack traces al cliente.

## Cómo ejecutar localmente

### Opción A — Docker Compose (recomendada)

```bash
docker-compose up --build
```

Esto levanta SQL Server, la API y el frontend. La API queda en `http://localhost:8080`, con
Swagger en `http://localhost:8080/swagger` (si `ASPNETCORE_ENVIRONMENT=Development`, que es el
valor por defecto del compose). El frontend queda en `http://localhost:4200`. Al arrancar, la
API aplica las migraciones automáticamente y siembra:

- Los roles `Asesor` y `Estudiante`.
- Un usuario Asesor de prueba: **`asesor.demo@eduapoyos.edu.co` / `Demo1234*`**.

Para reconstruir sin usar la caché de Docker (por ejemplo tras cambios en el `Dockerfile` o
dependencias que no se reflejan):

```bash
docker-compose build --no-cache
docker-compose up -d
```

### Opción B — Local con SQL Server LocalDB / instalado

1. Ajustar `ConnectionStrings:DefaultConnection` en `src/EduApoyos.Api/appsettings.Development.json`
   si no se usa LocalDB.
2. Ejecutar (las migraciones ya están en el repo, no hace falta generarlas de nuevo):

   ```bash
   dotnet restore EduApoyos.sln
   dotnet run --project src/EduApoyos.Api
   ```

   La API aplica las migraciones pendientes automáticamente al arrancar.

### Pruebas unitarias y cobertura

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Esto corre los tres proyectos de prueba (`Application.Tests`, `Infrastructure.Tests`,
`IntegrationTests`) y genera un reporte de cobertura por proyecto en formato Cobertura XML.

Para visualizarlo como reporte legible:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"tests\EduApoyos.Application.Tests\TestResults\**\coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:Html
```

**Resultado actual de cobertura en la capa Application: 93.4%** (mínimo exigido: 70%),
verificado con `dotnet-reportgenerator-globaltool`. Detalle relevante:

| Clase | Cobertura |
|---|---|
| `EstudianteService` | 95.5% |
| `SolicitudService` | 95.7% |

Los tests de `Application` cubren: creación de estudiante y solicitud (casos válidos e
inválidos), flujo completo de transición de estados (válidas, inválidas y estados finales),
autorización por recurso (estudiante vs. solicitud ajena), consultas por id/usuario/documento
con sus respectivos `NotFoundException`, listado paginado, y activación/desactivación lógica
de estudiantes con su regla de negocio (no se puede desactivar con solicitudes activas).

## Endpoints principales

| Método | Ruta | Rol |
|---|---|---|
| POST | `/api/auth/register` | Público |
| POST | `/api/auth/login` | Público |
| GET | `/api/estudiantes` | Asesor |
| POST | `/api/estudiantes` | Asesor |
| GET | `/api/estudiantes/{id}` | Asesor |
| GET | `/api/estudiantes/me` | Estudiante (propio) |
| GET | `/api/estudiantes/{id}/solicitudes` | Asesor / Propio |
| GET | `/api/solicitudes` | Asesor |
| POST | `/api/solicitudes` | Asesor / Estudiante |
| GET | `/api/solicitudes/{id}` | Asesor / Propio |
| PATCH | `/api/solicitudes/{id}/estado` | Asesor |

Documentación interactiva completa en Swagger una vez levantado el proyecto.

## Azure (documentado, sin desplegar)

| Servicio | Justificación |
|---|---|
| **Azure App Service** | Hosting del API (contenedor Linux, plan B1/S1 para dev/QA — escala vertical/horizontal simple sin gestionar VMs). |
| **Azure SQL Database** | Reemplazo gestionado de SQL Server. Tier Basic/S0 alcanza para el volumen esperado (una institución, no miles de solicitudes/segundo); se sube a Standard si el listado del asesor empieza a superar los 800 ms bajo carga real. |
| **Azure Blob Storage** | Para RF-04 si se agrega adjuntar/descargar constancias o soportes en PDF — evita guardar binarios en la base de datos. |
| **Azure Key Vault** | Gestión de `Jwt:Key` y cadena de conexión en producción, referenciados desde App Service vía Key Vault references en vez de appsettings. |

## Decisiones y pendientes

**Decisiones relevantes:** se priorizó un dominio rico (la entidad `SolicitudApoyo` controla su
propio ciclo de vida y genera su historial) sobre un modelo anémico con lógica en los servicios,
porque es la regla de negocio más importante del sistema y la que más valor tiene mantenida en
un solo lugar. Se reutilizó ASP.NET Core Identity en vez de reinventar hash de contraseñas. Las
migraciones se generaron de forma incremental (4 migraciones separadas, no una sola) para que el
historial de Git refleje la evolución real del modelo de datos, en línea con lo que pide la prueba
sobre commits progresivos.

**Qué queda pendiente por tiempo:** RF-04 (constancia) se resolvió generando un `.txt` en el
cliente — un PDF con membrete institucional real quedaría para una siguiente iteración; subir
la cobertura de `Infrastructure` (hoy el foco de pruebas está en `Application`, que es donde
vive la lógica de negocio, y en `IntegrationTests` para los flujos críticos de extremo a extremo);
y pruebas Jasmine/Karma en el frontend. Detalle completo del frontend (decisiones propias,
sistema de diseño) en `frontend/README.md`.