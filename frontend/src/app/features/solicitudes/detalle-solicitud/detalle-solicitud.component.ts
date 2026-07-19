import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { SolicitudesService } from '../../../core/services/solicitudes.service';
import { AuthService } from '../../../core/services/auth.service';
import { SweetAlertService } from '../../../core/services/sweet alert.service';
import {
  ESTADO_LABEL,
  EstadoSolicitud,
  RolUsuario,

  TIPO_APOYO_LABEL,
  TRANSICIONES_VALIDAS,
} from '../../../core/models';
import {   SolicitudDetalle} from '../../../core/models/solicitud.models';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-detalle-solicitud',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe, DatePipe, RouterLink, FormsModule, StatusBadgeComponent, SpinnerComponent],
  templateUrl: './detalle-solicitud.component.html',
  styleUrl: './detalle-solicitud.component.scss',
})
export class DetalleSolicitudComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly solicitudesService = inject(SolicitudesService);
  private readonly auth = inject(AuthService);
  private readonly sweetAlert = inject(SweetAlertService);

  readonly tipoLabel = TIPO_APOYO_LABEL;
  readonly estadoLabel = ESTADO_LABEL;
  readonly esAsesor = this.auth.rolActual() === RolUsuario.Asesor;

  readonly cargando = signal(true);
  readonly actualizando = signal(false);
  readonly solicitud = signal<SolicitudDetalle | null>(null);

  nuevoEstado: EstadoSolicitud | null = null;
  observacion = '';

  constructor() {
    this.cargar();
  }

  private cargar(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.cargando.set(true);
    this.solicitudesService
      .obtenerDetalle(id)
      .pipe(finalize(() => this.cargando.set(false)))
      .subscribe((detalle) => this.solicitud.set(detalle));
  }

  transicionesDisponibles(): EstadoSolicitud[] {
    const actual = this.solicitud()?.estado;
    return actual != null ? TRANSICIONES_VALIDAS[actual] : [];
  }

  async confirmarCambioEstado(): Promise<void> {
    const actual = this.solicitud();
    if (!actual || this.nuevoEstado == null) return;

    const etiquetaEstado = this.estadoLabel[this.nuevoEstado];
    const confirmado = await this.sweetAlert.confirmar(
      '¿Cambiar el estado de la solicitud?',
      `El nuevo estado será "${etiquetaEstado}". Esta acción quedará registrada.`,
      { icono: 'question', confirmText: 'Sí, cambiar' }
    );
    if (!confirmado) {
      return;
    }

    this.actualizando.set(true);
    this.solicitudesService
      .cambiarEstado(actual.id, { nuevoEstado: this.nuevoEstado, observacion: this.observacion || null })
      .pipe(finalize(() => this.actualizando.set(false)))
      .subscribe({
        next: (actualizada) => {
          this.solicitud.set(actualizada);
          this.nuevoEstado = null;
          this.observacion = '';
          this.sweetAlert.exito('Estado actualizado correctamente.');
        },
        error: () => {
          // El interceptor global ya muestra el detalle del ProblemDetails en un toast.
        },
      });
  }
}
