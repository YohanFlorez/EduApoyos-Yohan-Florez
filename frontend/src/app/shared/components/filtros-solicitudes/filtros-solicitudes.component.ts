import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import {
  ESTADO_LABEL,
  EstadoSolicitud,
  TIPO_APOYO_LABEL,
  TipoApoyo,
} from '../../../core/models';

/** Criterios de filtro de negocio, sin paginación — la paginación (si aplica)
 *  sigue siendo responsabilidad de quien consume este componente. */
export interface CriteriosFiltroSolicitudes {
  estado: EstadoSolicitud | null;
  tipoApoyo: TipoApoyo | null;
  fechaDesde: string | null;
  fechaHasta: string | null;
}

/** Reutilizable en cualquier pantalla que liste solicitudes (panel del
 *  asesor, portal del estudiante, etc.) — por eso vive en `shared`. */
export const CRITERIOS_FILTRO_SOLICITUDES_INICIALES: CriteriosFiltroSolicitudes = {
  estado: null,
  tipoApoyo: null,
  fechaDesde: null,
  fechaHasta: null,
};


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


  readonly totalResultados = input<number>(0);


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
