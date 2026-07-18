import { ChangeDetectionStrategy, Component, computed, model } from '@angular/core';

export type FiltroEstado = 'todos' | 'activos' | 'inactivos';


@Component({
  selector: 'app-estudiantes-filtros',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [],
  templateUrl: './estudiantes-filtros.component.html',
  styleUrl: './estudiantes-filtros.component.scss',
})
export class EstudiantesFiltrosComponent {
  readonly tipoDocumento = model<string>('TODOS');
  readonly numeroDocumento = model<string>('');
  readonly estado = model<FiltroEstado>('todos');

  readonly hayFiltrosActivos = computed(
    () =>
      this.tipoDocumento() !== 'TODOS' ||
      this.numeroDocumento().trim() !== '' ||
      this.estado() !== 'todos'
  );

  onTipoDocumentoChange(valor: string): void {
    this.tipoDocumento.set(valor);
  }

  onNumeroDocumentoChange(valor: string): void {
    this.numeroDocumento.set(valor);
  }

  onEstadoChange(valor: FiltroEstado): void {
    this.estado.set(valor);
  }

  limpiar(): void {
    this.tipoDocumento.set('TODOS');
    this.numeroDocumento.set('');
    this.estado.set('todos');
  }
}
