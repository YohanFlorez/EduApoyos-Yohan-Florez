import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { noAuthGuard } from './core/guards/no-auth.guard';
import { RolUsuario } from './core/models';
import { APP_ROUTES } from './core/config/app-routes.config';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [noAuthGuard],
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'registro',
    canActivate: [noAuthGuard],
    loadComponent: () => import('./features/auth/registro/registro.component').then((m) => m.RegistroComponent),
  },

  // ---- Asesor ----
  {
    path: 'solicitudes',
    canActivate: [authGuard, roleGuard],
    data: { roles: [RolUsuario.Asesor] },
    loadComponent: () =>
      import('./features/asesor/panel-solicitudes/panel-solicitudes.component').then((m) => m.PanelSolicitudesComponent),
  },
  {
    path: 'estudiantes',
    canActivate: [authGuard, roleGuard],
    data: { roles: [RolUsuario.Asesor] },
    loadComponent: () =>
      import('./features/asesor/estudiantes/estudiantes-listado/estudiantes-listado.component').then((m) => m.EstudiantesListadoComponent),
  },
  {
    path: 'estudiantes/:id/solicitudes',
    canActivate: [authGuard, roleGuard],
    data: { roles: [RolUsuario.Asesor] },
    loadComponent: () =>
      import('./features/asesor/estudiante-solicitudes/estudiante-solicitudes.component').then(
        (m) => m.EstudianteSolicitudesComponent,
      ),
  },

  // ---- Estudiante ----
  {
    path: 'portal',
    canActivate: [authGuard, roleGuard],
    data: { roles: [RolUsuario.Estudiante] },
    loadComponent: () =>
      import('./features/estudiante/portal-estudiante/portal-estudiante.component').then((m) => m.PortalEstudianteComponent),
  },

  // ---- Compartidas (Asesor y Estudiante) ----
  {
    path: 'solicitudes/nueva',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/solicitudes/formulario-solicitud/formulario-solicitud.component').then(
        (m) => m.FormularioSolicitudComponent,
      ),
  },
  {
    path: 'solicitudes/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/solicitudes/detalle-solicitud/detalle-solicitud.component').then(
        (m) => m.DetalleSolicitudComponent,
      ),
  },
  {
    path: 'perfil',
    canActivate: [authGuard],
    loadComponent: () => import('./features/perfil/perfil.component').then((m) => m.PerfilComponent),
  },

  { path: '', pathMatch: 'full', redirectTo: APP_ROUTES.LOGIN },
  { path: '**', redirectTo: APP_ROUTES.LOGIN },
];
