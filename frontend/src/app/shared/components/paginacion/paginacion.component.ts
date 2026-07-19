import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';


 //Componente de paginación genérico y reutilizable

@Component({
  selector: 'app-paginacion',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [],
  templateUrl: './paginacion.component.html',
  styleUrl: './paginacion.component.scss',
})
export class PaginacionComponent {
  readonly paginaActual = input.required<number>();
  readonly totalPaginas = input<number>(1);

  readonly cambiarPagina = output<number>();

  readonly esPrimeraPagina = computed(() => this.paginaActual() <= 1);
  readonly esUltimaPagina = computed(() => this.paginaActual() >= this.totalPaginas());

  anterior(): void {
    if (!this.esPrimeraPagina()) {
      this.cambiarPagina.emit(this.paginaActual() - 1);
    }
  }

  siguiente(): void {
    if (!this.esUltimaPagina()) {
      this.cambiarPagina.emit(this.paginaActual() + 1);
    }
  }
}
