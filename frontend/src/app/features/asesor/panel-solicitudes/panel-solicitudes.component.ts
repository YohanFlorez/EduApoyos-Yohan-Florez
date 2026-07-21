import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SolicitudesService } from '../../../core/services/solicitudes.service';
import { FiltroSolicitudes, TIPO_APOYO_LABEL } from '../../../core/models';
import { SolicitudListItem } from '../../../core/models/solicitud.models';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { PaginacionComponent } from '../../../shared/components/paginacion/paginacion.component';
import {
  CriteriosFiltroSolicitudes,
  FiltrosSolicitudesComponent,
} from '../filtros-solicitudes/filtros-solicitudes.component';

const PAGE_SIZE = 10;

const CRITERIOS_INICIALES: CriteriosFiltroSolicitudes = {
  estado: null,
  tipoApoyo: null,
  fechaDesde: null,
  fechaHasta: null,
};

/**
 * Ahora es un orquestador delgado: mantiene la página actual y los
 * criterios de filtro vigentes, y delega la captura de filtros al
 * componente hijo. Toda recarga pasa por un único Subject + switchMap,
 * lo que cancela automáticamente cualquier petición anterior en vuelo
 * (evita la condición de carrera del componente original).
 */
@Component({
  selector: 'app-panel-solicitudes',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    RouterLink,
    StatusBadgeComponent,
    SpinnerComponent,
    CurrencyPipe,
    DatePipe,
    FiltrosSolicitudesComponent,
    PaginacionComponent,
  ],
  templateUrl: './panel-solicitudes.component.html',
  styleUrl: './panel-solicitudes.component.scss',
})
export class PanelSolicitudesComponent {
  private readonly solicitudesService = inject(SolicitudesService);
  private readonly destroyRef = inject(DestroyRef);

  readonly tipoLabel = TIPO_APOYO_LABEL;

  readonly cargando = signal(false);
  readonly solicitudes = signal<SolicitudListItem[]>([]);
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);
  readonly paginaActual = signal(1);

  private criterios: CriteriosFiltroSolicitudes = { ...CRITERIOS_INICIALES };
  private readonly recargar$ = new Subject<void>();

  constructor() {
    this.recargar$
      .pipe(
        tap(() => this.cargando.set(true)),
        // switchMap cancela la petición anterior si llega una nueva orden de
        // recarga antes de que responda — evita que una respuesta lenta y
        // obsoleta sobrescriba datos más recientes.
        switchMap(() => this.solicitudesService.listar(this.construirFiltro())),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((resultado) => {
        this.solicitudes.set(resultado.items);
        this.totalCount.set(resultado.totalCount);
        this.totalPages.set(resultado.totalPages);
        this.cargando.set(false);
      });

    this.recargar$.next();
  }

  private construirFiltro(): FiltroSolicitudes {
    return {
      ...this.criterios,
      pageNumber: this.paginaActual(),
      pageSize: PAGE_SIZE,
    };
  }

  onAplicarFiltros(criterios: CriteriosFiltroSolicitudes): void {
    this.criterios = criterios;
    this.paginaActual.set(1);
    this.recargar$.next();
  }

  onCambiarPagina(pagina: number): void {
    this.paginaActual.set(pagina);
    this.recargar$.next();
  }
}
