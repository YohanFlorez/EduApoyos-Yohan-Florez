import { RolUsuario } from '../models';
import { APP_ROUTES } from './app-routes.config';

const RUTAS_INICIO_POR_ROL: Record<RolUsuario, string> = {
  [RolUsuario.Estudiante]: APP_ROUTES.PORTAL,
  [RolUsuario.Asesor]: APP_ROUTES.SOLICITUDES,
};

export function obtenerRutaInicioPorRol(rol: RolUsuario | null): string {
  if (rol === null) return APP_ROUTES.LOGIN;
  return RUTAS_INICIO_POR_ROL[rol] ?? APP_ROUTES.LOGIN;
}
