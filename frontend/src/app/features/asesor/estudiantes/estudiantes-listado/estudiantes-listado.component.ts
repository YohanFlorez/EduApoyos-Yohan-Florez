import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { EstudiantesService } from '../../../../core/services/estudiantes.service';
import { SweetAlertService } from '../../../../core/services/sweet alert.service';
import { Estudiante } from '../../../../core/models/estudiante.models';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import {
  EstudiantesFiltrosComponent,
  FiltroEstado,
} from '../estudiantes-filtros/estudiantes-filtros.component';
import { EstudianteFormularioComponent } from '../estudiante-formulario/estudiante-formulario.component';

@Component({
  selector: 'app-estudiantes-listado',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, SpinnerComponent, EstudiantesFiltrosComponent, EstudianteFormularioComponent],
  templateUrl: './estudiantes-listado.component.html',
  styleUrl: './estudiantes-listado.component.scss',
})
export class EstudiantesListadoComponent {
  private readonly estudiantesService = inject(EstudiantesService);
  private readonly sweetAlert = inject(SweetAlertService);

  readonly cargando = signal(true);
  readonly eliminando = signal<string | null>(null);
  readonly mostrarFormulario = signal(false);
  readonly editando = signal<Estudiante | null>(null);
  readonly estudiantes = signal<Estudiante[]>([]);


  readonly filtroTipoDocumento = signal<string>('TODOS');
  readonly filtroNumeroDocumento = signal<string>('');
  readonly filtroEstado = signal<FiltroEstado>('todos');

  readonly estudiantesFiltrados = computed(() => {
    const tipo = this.filtroTipoDocumento();
    const numero = this.filtroNumeroDocumento().trim().toLowerCase();
    const estado = this.filtroEstado();

    return this.estudiantes().filter((e) => {
      const coincideTipo = tipo === 'TODOS' || e.tipoDocumento === tipo;
      const coincideNumero = numero === '' || e.numeroDocumento.toLowerCase().includes(numero);
      const coincideEstado =
        estado === 'todos' ||
        (estado === 'activos' && e.activo) ||
        (estado === 'inactivos' && !e.activo);

      return coincideTipo && coincideNumero && coincideEstado;
    });
  });

  constructor() {
    this.cargar();
  }

  private cargar(): void {
    this.cargando.set(true);
    this.estudiantesService
      .listar(1, 50)
      .pipe(finalize(() => this.cargando.set(false)))
      .subscribe((resultado) => this.estudiantes.set(resultado.items));
  }

  toggleFormulario(): void {
    if (this.mostrarFormulario()) {
      this.cerrarFormulario();
    } else {
      this.editando.set(null);
      this.mostrarFormulario.set(true);
    }
  }

  editar(estudiante: Estudiante): void {
    this.editando.set(estudiante);
    this.mostrarFormulario.set(true);
  }

  private cerrarFormulario(): void {
    this.mostrarFormulario.set(false);
    this.editando.set(null);
  }

  onGuardado(): void {
    this.cerrarFormulario();
    this.cargar();
  }

  async desactivar(estudiante: Estudiante): Promise<void> {
    const confirmado = await this.sweetAlert.confirmarDesactivacion(
      `al estudiante ${estudiante.tipoDocumento} ${estudiante.numeroDocumento}`
    );
    if (!confirmado) {
      return;
    }

    this.eliminando.set(estudiante.id);
    this.estudiantesService
      .desactivar(estudiante.id)
      .pipe(finalize(() => this.eliminando.set(null)))
      .subscribe({
        next: () => {
          this.sweetAlert.exito('Estudiante desactivado correctamente.');
          this.cargar();
        },
        error: () => {
          this.sweetAlert.error('No se pudo desactivado el estudiante.');
        },
      });
  }
}
