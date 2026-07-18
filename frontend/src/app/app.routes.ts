import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { RolUsuario } from './core/models';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'registro',
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
    loadComponent: () => import('./features/perfil/perfil.component').then(m => m.PerfilComponent),
    canActivate: [authGuard], // el guard que ya uses para rutas autenticadas
  },

  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: '**', redirectTo: 'login' },
];
