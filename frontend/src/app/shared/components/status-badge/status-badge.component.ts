import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { NgClass } from '@angular/common';
import { ESTADO_LABEL, EstadoSolicitud } from '../../../core/models';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<span class="sello" [ngClass]="claseEstado">{{ etiqueta }}</span>`,
  imports: [NgClass],
})
export class StatusBadgeComponent {
  @Input({ required: true }) estado!: EstadoSolicitud;

  get etiqueta(): string {
    return ESTADO_LABEL[this.estado];
  }

  get claseEstado(): string {
    switch (this.estado) {
      case EstadoSolicitud.Pendiente:
        return 'sello-pendiente';
      case EstadoSolicitud.EnRevision:
        return 'sello-revision';
      case EstadoSolicitud.Aprobada:
        return 'sello-aprobada';
      case EstadoSolicitud.Rechazada:
        return 'sello-rechazada';
    }
  }
}
