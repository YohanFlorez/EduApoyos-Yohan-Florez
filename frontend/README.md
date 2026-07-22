# EduApoyos — Frontend (Angular 18)

SPA que consume la API de `EduApoyos.Api`. Standalone components, señales (`signal`),
Reactive Forms, guards funcionales y un interceptor de JWT.

## Prerrequisitos

| Herramienta | Versión mínima |
|---|---|
| Node.js | 20.x LTS (requerido por Angular 18) |
| npm | 10.x (incluido con Node 20) |
| Angular CLI | No es obligatorio instalarlo global; `npm start` usa el CLI local del proyecto |

Verificar versión instalada:
```bash
node -v
npm -v
```

## Vistas implementadas

| Vista | Ruta | Rol |
|---|---|---|
| Login | `/login` | Público |
| Registro | `/registro` | Público |
| Panel del asesor (tabla + filtros + paginación) | `/solicitudes` | Asesor |
| Estudiantes (listado + alta) | `/estudiantes` | Asesor |
| Formulario de nueva solicitud | `/solicitudes/nueva` | Asesor / Estudiante |
| Detalle de solicitud (+ cambio de estado + historial) | `/solicitudes/:id` | Asesor / Propio |
| Portal del estudiante | `/portal` | Estudiante |

## Diseño

Sistema de diseño propio ("libro mayor académico"): tipografía serif (Source Serif 4) para
títulos + sans (IBM Plex Sans) para texto + mono (IBM Plex Mono) para datos/metadatos, sobre
una paleta fría de papel con azul institucional y un acento bronce usado como "sello" en los
estados de la solicitud (`Pendiente`, `En revisión`, `Aprobada`, `Rechazada`). Tokens y clases
utilitarias en `src/styles.scss` (sin Angular Material — CSS propio para control total del
resultado visual).

## Arquitectura del código

```
src/app/
  core/
    models/        → interfaces y enums que reflejan los DTOs del backend
    services/       → AuthService (signals + localStorage), EstudiantesService, SolicitudesService, ToastService
    interceptors/   → jwtInterceptor (agrega Authorization), errorInterceptor (ProblemDetails → toast, maneja 401)
    guards/         → authGuard, roleGuard (funcionales, basados en route.data.roles)
  layout/shell/     → header con navegación por rol + logout + host de notificaciones
  shared/components/→ status-badge (el "sello"), spinner, toast-host
  features/
    auth/           → login, registro
    asesor/         → panel-solicitudes, estudiantes, estudiante-solicitudes
    estudiante/     → portal-estudiante
    solicitudes/    → formulario-solicitud, detalle-solicitud (compartidos por rol)
```

Todas las rutas de `features` usan `loadComponent` (lazy loading por ruta).

## Cómo ejecutar

### Desarrollo (con la API corriendo en `localhost:8080`)

```bash
npm install
npm start
```

`npm start` corre `ng serve` con `proxy.conf.json`, que reenvía `/api/*` hacia
`http://localhost:8080`, así el frontend nunca necesita conocer la URL absoluta del backend.
Abrir `http://localhost:4200`.

### Con Docker Compose (desde la raíz del repo)

```bash
docker-compose up --build
```

Levanta SQL Server + API + este frontend (servido por nginx en `http://localhost:4200`,
con nginx haciendo de proxy hacia el contenedor `api` en las rutas `/api/*`). A diferencia del
modo desarrollo, aquí nginx —no `proxy.conf.json`— es quien resuelve el reenvío de `/api/*`
hacia el contenedor `api` dentro de la red interna de Docker.

### Build de producción

```bash
npm run build
```

El resultado se genera en `dist/` (usado por el `Dockerfile` de este proyecto para armar la
imagen servida por nginx).

Nota: el *inlining* de Google Fonts en build de producción está deshabilitado
(`angular.json → optimization.fonts: false`) para que el build no dependa de acceso a
`fonts.googleapis.com` en entornos de CI con red restringida; las fuentes igual se cargan en
runtime vía `<link>` en `index.html`.

## Scripts npm disponibles

| Script | Qué hace |
|---|---|
| `npm start` | `ng serve` con proxy hacia la API en `localhost:8080` |
| `npm run build` | Build de producción en `dist/` |

## Variables de entorno

Este frontend **no requiere variables de entorno propias**: en desarrollo la URL de la API se
resuelve vía `proxy.conf.json`, y en Docker vía la configuración de nginx. El archivo
`.env.example` en la raíz del repositorio corresponde a la configuración del backend/SQL Server
(cadena de conexión, `Jwt:Key`), no al frontend.

## Cómo probar rápido (con datos de ejemplo)

Usando el usuario Asesor sembrado por el backend al arrancar:

- **Usuario:** `asesor.demo@eduapoyos.edu.co`
- **Contraseña:** `Demo1234*`

Con esa cuenta se puede: crear un estudiante, crear una solicitud a su nombre, y cambiar su
estado desde el detalle de la solicitud. Para probar el portal del estudiante, registrar un
usuario con rol Estudiante desde `/registro` y asociarlo a un estudiante existente.


## Decisiones relevantes

- **Sin Angular Material**: se optó por un sistema de diseño propio en CSS plano para lograr
  una identidad visual distintiva y evitar el look genérico de un tema Material por defecto.
- **Guards funcionales** (`authGuard`, `roleGuard`) en vez de clases, siguiendo el estilo
  recomendado desde Angular 15+.
- **Señales** para el estado de sesión y de carga en vez de solo Observables/`async` pipe,
  aprovechando el modelo de reactividad más reciente de Angular.
- **`GET /api/estudiantes/me`** (agregado al backend): el estudiante autenticado no conocía su
  propio `EstudianteId` a partir del JWT (que solo trae `usuarioId`); este endpoint resuelve esa
  brecha para el portal y el formulario de nueva solicitud.
- **Constancia de solicitud (requerimiento funcional 4)**: se genera un `.txt` en el cliente con los datos de la
  solicitud (`Blob` + descarga), en vez de un endpoint de PDF en el backend, priorizando cerrar
  el flujo completo del estudiante dentro del tiempo disponible. Pendiente si se requiere un PDF
  con membrete institucional real.

## Pendiente por tiempo

- Pruebas con Jasmine/Karma (no incluidas; se priorizaron las pruebas del backend, que cubren la
  lógica de negocio).
- Edición/eliminación de estudiantes (hoy solo alta + consulta, cubre lo mínimo de RF-02).
- Manejo de refresh token (hoy la sesión expira con el JWT y pide iniciar sesión de nuevo).
