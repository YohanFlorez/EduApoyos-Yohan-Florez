import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormControl, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { debounceTime, distinctUntilChanged, finalize, switchMap } from 'rxjs';
import { EstudiantesService } from '../../../core/services/estudiantes.service';
import { SweetAlertService } from '../../../core/services/sweet alert.service';
import { Estudiante, UsuarioPendiente } from '../../../core/models/estudiante.models';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';

type FiltroEstado = 'todos' | 'activos' | 'inactivos';

@Component({
  selector: 'app-estudiantes-listado',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, SpinnerComponent],
  templateUrl: './estudiantes-listado.component.html',
  styleUrl: './estudiantes-listado.component.scss',
})
export class EstudiantesListadoComponent {
  private readonly fb = inject(FormBuilder);
  private readonly estudiantesService = inject(EstudiantesService);
  private readonly sweetAlert = inject(SweetAlertService);

  readonly cargando = signal(true);
  readonly guardando = signal(false);
  readonly eliminando = signal<string | null>(null);
  readonly mostrarFormulario = signal(false);
  readonly estudiantes = signal<Estudiante[]>([]);

  // --- Filtros ---
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

  readonly hayFiltrosActivos = computed(
    () =>
      this.filtroTipoDocumento() !== 'TODOS' ||
      this.filtroNumeroDocumento().trim() !== '' ||
      this.filtroEstado() !== 'todos'
  );

  limpiarFiltros(): void {
    this.filtroTipoDocumento.set('TODOS');
    this.filtroNumeroDocumento.set('');
    this.filtroEstado.set('todos');
  }

  // --- Modo edición ---
  readonly editando = signal<Estudiante | null>(null);

  // --- Modal de búsqueda ---
  readonly mostrarModal = signal(false);
  readonly buscando = signal(false);
  readonly resultadosBusqueda = signal<UsuarioPendiente[]>([]);
  readonly nombreSeleccionado = signal<string>('');
  readonly busquedaControl = new FormControl('', { nonNullable: true });

  readonly formulario = this.fb.nonNullable.group({
    usuarioId: ['', [Validators.required]],
    numeroDocumento: ['', [Validators.required]],
    tipoDocumento: ['CC', [Validators.required]],
    programaAcademico: ['', [Validators.required]],
    semestre: [1, [Validators.required, Validators.min(1), Validators.max(20)]],
  });

  constructor() {
    this.cargar();

    this.busquedaControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((texto) => {
          this.buscando.set(true);
          return this.estudiantesService
            .listarPendientes(texto)
            .pipe(finalize(() => this.buscando.set(false)));
        })
      )
      .subscribe({
        next: (resultados) => this.resultadosBusqueda.set(resultados),
        error: () => this.resultadosBusqueda.set([]),
      });
  }

  private cargar(): void {
    this.cargando.set(true);
    this.estudiantesService
      .listar(1, 50)
      .pipe(finalize(() => this.cargando.set(false)))
      .subscribe((resultado) => this.estudiantes.set(resultado.items));
  }

  // --- Formulario: crear / editar ---

  toggleFormulario(): void {
    if (this.mostrarFormulario()) {
      this.cerrarFormulario();
    } else {
      this.mostrarFormulario.set(true);
    }
  }

  private cerrarFormulario(): void {
    this.mostrarFormulario.set(false);
    this.editando.set(null);
    this.nombreSeleccionado.set('');
    this.formulario.reset({ tipoDocumento: 'CC', semestre: 1 });
    this.formulario.controls.usuarioId.enable();
  }

  editar(estudiante: Estudiante): void {
    this.editando.set(estudiante);

    this.formulario.setValue({
      usuarioId: estudiante.usuarioId,
      numeroDocumento: estudiante.numeroDocumento,
      tipoDocumento: estudiante.tipoDocumento,
      programaAcademico: estudiante.programaAcademico,
      semestre: estudiante.semestre,
    });

    this.formulario.controls.usuarioId.disable();
    this.mostrarFormulario.set(true);
  }

  async eliminar(estudiante: Estudiante): Promise<void> {
    const confirmado = await this.sweetAlert.confirmarEliminacion(
      `al estudiante ${estudiante.tipoDocumento} ${estudiante.numeroDocumento}`
    );
    if (!confirmado) {
      return;
    }

    this.eliminando.set(estudiante.id);
    this.estudiantesService
      .eliminar(estudiante.id)
      .pipe(finalize(() => this.eliminando.set(null)))
      .subscribe({
        next: () => {
          this.sweetAlert.exito('Estudiante eliminado correctamente.');
          this.cargar(); // recarga para reflejar el cambio de estado (activo: false)
        },
        error: () => {
          this.sweetAlert.error('No se pudo eliminar el estudiante.');
        },
      });
  }

  abrirModal(): void {
    this.mostrarModal.set(true);
    this.busquedaControl.setValue('', { emitEvent: false });
    this.cargarPendientes('');
  }

  cerrarModal(): void {
    this.mostrarModal.set(false);
  }

  seleccionarUsuario(usuario: UsuarioPendiente): void {
    this.formulario.patchValue({ usuarioId: usuario.id });
    this.nombreSeleccionado.set(`${usuario.nombreCompleto} (${usuario.email})`);
    this.mostrarModal.set(false);
  }

  async guardar(): Promise<void> {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    const enEdicion = this.editando();

    if (enEdicion) {
      const confirmado = await this.sweetAlert.confirmar(
        '¿Actualizar estudiante?',
        'Se guardarán los cambios realizados.',
        { icono: 'question', confirmText: 'Sí, actualizar' }
      );
      if (!confirmado) {
        return;
      }
    }

    this.guardando.set(true);
    const valores = this.formulario.getRawValue();

    const peticion = enEdicion
      ? this.estudiantesService.actualizar(enEdicion.id, valores)
      : this.estudiantesService.crear(valores);

    peticion.pipe(finalize(() => this.guardando.set(false))).subscribe({
      next: () => {
        this.sweetAlert.exito(
          enEdicion ? 'Estudiante actualizado correctamente.' : 'Estudiante creado correctamente.'
        );
        this.cerrarFormulario();
        this.cargar();
      },
      error: () => {
        this.sweetAlert.error(
          enEdicion ? 'No se pudo actualizar el estudiante.' : 'No se pudo crear el estudiante.'
        );
      },
    });
  }

  get f() { return this.formulario.controls; }

  private cargarPendientes(texto: string): void {
    this.buscando.set(true);
    this.estudiantesService
      .listarPendientes(texto)
      .pipe(finalize(() => this.buscando.set(false)))
      .subscribe({
        next: (resultados) => this.resultadosBusqueda.set(resultados),
        error: () => this.resultadosBusqueda.set([]),
      });
  }
}
