import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';
import { RolUsuario } from '../models';
import { obtenerRutaInicioPorRol } from '../config/role-home.config';

/** Restringe una ruta a los roles indicados en `data: { roles: [RolUsuario.Asesor] }`. */
export const roleGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const toast = inject(ToastService);

  const rolesPermitidos = route.data['roles'] as RolUsuario[] | undefined;
  const rolActual = authService.rolActual();

  if (!rolesPermitidos || (rolActual !== null && rolesPermitidos.includes(rolActual))) {
    return true;
  }

  toast.info('No tiene permisos para acceder a esa sección.');
  return router.createUrlTree([obtenerRutaInicioPorRol(rolActual)]);
};
