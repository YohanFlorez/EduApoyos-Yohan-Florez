import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { obtenerRutaInicioPorRol } from '../config/role-home.config';

/** bloquea el acceso a rutas públicas (login, registro) si ya hay sesión activa. */
export const noAuthGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.estaAutenticado()) {
    return true;
  }

  return router.createUrlTree([obtenerRutaInicioPorRol(authService.rolActual())]);
};
