import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import {
  ESTADO_LABEL,
  EstadoSolicitud,
  TIPO_APOYO_LABEL,
  TipoApoyo,
} from '../../../core/models';

/** Criterios de filtro de negocio, sin paginación (esa es responsabilidad
 *  del componente padre, igual que en el backend ISolicitudRepository). */
export interface CriteriosFiltroSolicitudes {
  estado: EstadoSolicitud | null;
  tipoApoyo: TipoApoyo | null;
  fechaDesde: string | null;
  fechaHasta: string | null;
}

/**
 * Responsabilidad única: capturar los criterios de filtro que el usuario
 * quiere aplicar y emitirlos. No conoce la lista de solicitudes ni cómo
 * se pagina o se llama al servicio — eso es del padre.
 */
@Component({
  selector: 'app-filtros-solicitudes',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule],
  templateUrl: './filtros-solicitudes.component.html',
  styleUrl: './filtros-solicitudes.component.scss',
})
export class FiltrosSolicitudesComponent {
  private readonly fb = inject(FormBuilder);

  /** Solo para mostrar el contador de resultados junto a los filtros. */
  readonly totalResultados = input<number>(0);

  /** Se emite tanto al presionar "Filtrar" como al "Limpiar". */
  readonly aplicar = output<CriteriosFiltroSolicitudes>();

  readonly estadoLabel = ESTADO_LABEL;
  readonly tipoLabel = TIPO_APOYO_LABEL;
  readonly estados = Object.values(EstadoSolicitud).filter((v) => typeof v === 'number') as EstadoSolicitud[];
  readonly tipos = Object.values(TipoApoyo).filter((v) => typeof v === 'number') as TipoApoyo[];

  readonly formulario = this.fb.group({
    estado: this.fb.control<EstadoSolicitud | null>(null),
    tipoApoyo: this.fb.control<TipoApoyo | null>(null),
    fechaDesde: this.fb.control<string | null>(null),
    fechaHasta: this.fb.control<string | null>(null),
  });

  filtrar(): void {
    this.aplicar.emit(this.formulario.getRawValue());
  }

  limpiar(): void {
    this.formulario.reset({ estado: null, tipoApoyo: null, fechaDesde: null, fechaHasta: null });
    this.aplicar.emit(this.formulario.getRawValue());
  }
}
