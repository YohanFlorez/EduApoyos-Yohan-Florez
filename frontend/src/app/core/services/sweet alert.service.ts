import { Injectable } from '@angular/core';
import Swal, { SweetAlertIcon } from 'sweetalert2';

/**
 * Servicio centralizado para mostrar alertas y confirmaciones con SweetAlert2.
 * Úsalo en cualquier componente inyectándolo con `inject(SweetAlertService)`.
 */
@Injectable({ providedIn: 'root' })
export class SweetAlertService {

  /**
   * Confirmación genérica (Sí/No). Devuelve una Promise<boolean>.
   * Ejemplo:
   *   const ok = await this.sweetAlert.confirmar('¿Eliminar estudiante?', 'Esta acción no se puede deshacer.');
   *   if (!ok) return;
   */
  async confirmar(
    titulo: string,
    texto = '',
    opciones?: { icono?: SweetAlertIcon; confirmText?: string; cancelText?: string }
  ): Promise<boolean> {
    const resultado = await Swal.fire({
      title: titulo,
      text: texto,
      icon: opciones?.icono ?? 'warning',
      showCancelButton: true,
      confirmButtonText: opciones?.confirmText ?? 'Sí, continuar',
      cancelButtonText: opciones?.cancelText ?? 'Cancelar',
      reverseButtons: true,
      focusCancel: true,
    });
    return resultado.isConfirmed;
  }

  /** Confirmación específica para eliminar (icono y textos ya orientados a esa acción). */
  async confirmarEliminacion(entidad: string): Promise<boolean> {
    return this.confirmar(
      `¿Eliminar ${entidad}?`,
      'Esta acción no se puede deshacer.',
      { icono: 'warning', confirmText: 'Sí, eliminar', cancelText: 'Cancelar' }
    );
  }

  /** Notificación de éxito (auto-cierra). */
  exito(mensaje: string, titulo = 'Listo'): void {
    Swal.fire({
      title: titulo,
      text: mensaje,
      icon: 'success',
      timer: 2000,
      showConfirmButton: false,
    });
  }

  /** Notificación de error. */
  error(mensaje: string, titulo = 'Ocurrió un error'): void {
    Swal.fire({
      title: titulo,
      text: mensaje,
      icon: 'error',
      confirmButtonText: 'Entendido',
    });
  }

  /** Notificación de advertencia simple (sin confirmación). */
  advertencia(mensaje: string, titulo = 'Atención'): void {
    Swal.fire({
      title: titulo,
      text: mensaje,
      icon: 'warning',
      confirmButtonText: 'Ok',
    });
  }
}
