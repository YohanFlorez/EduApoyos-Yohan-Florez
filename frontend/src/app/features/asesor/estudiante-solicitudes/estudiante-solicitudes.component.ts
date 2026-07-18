import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { EstudiantesService } from '../../../core/services/estudiantes.service';
import { SolicitudListItem } from '../../../core/models/solicitud.models';
import {  TIPO_APOYO_LABEL } from '../../../core/models';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-estudiante-solicitudes',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe, DatePipe, RouterLink, StatusBadgeComponent, SpinnerComponent],
  templateUrl: './estudiante-solicitudes.component.html',
})
export class EstudianteSolicitudesComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly estudiantesService = inject(EstudiantesService);

  readonly tipoLabel = TIPO_APOYO_LABEL;
  readonly cargando = signal(true);
  readonly solicitudes = signal<SolicitudListItem[]>([]);

  constructor() {
  const estudianteId = this.route.snapshot.paramMap.get('id')!;
  this.estudiantesService
    .listarSolicitudes(estudianteId, 1, 50)
    .pipe(finalize(() => this.cargando.set(false)))
    .subscribe((resultado) => {
      console.log('Solicitudes recibidas:', resultado.items); // 👈 agrega esto
      this.solicitudes.set(resultado.items);
    });
}
}
