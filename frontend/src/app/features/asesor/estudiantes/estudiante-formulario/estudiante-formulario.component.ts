import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { EstudiantesService } from '../../../../core/services/estudiantes.service';
import { SweetAlertService } from '../../../../core/services/sweet alert.service';
import { Estudiante, UsuarioPendiente } from '../../../../core/models/estudiante.models';
import { BuscarUsuarioModalComponent } from '../buscar-usuario-modal/buscar-usuario-modal.component';
import { ProblemDetails } from '../../../../core/models'; // ajusta la ruta según tu proyecto

@Component({
  selector: 'app-estudiante-formulario',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, BuscarUsuarioModalComponent],
  templateUrl: './estudiante-formulario.component.html',
  styleUrl: './estudiante-formulario.component.scss',
})
export class EstudianteFormularioComponent {
  private readonly fb = inject(FormBuilder);
  private readonly estudiantesService = inject(EstudiantesService);
  private readonly sweetAlert = inject(SweetAlertService);


  readonly estudiante = input<Estudiante | null>(null);

  readonly guardado = output<void>();
  readonly cancelado = output<void>();

  readonly editando = computed(() => this.estudiante() !== null);
  readonly guardando = signal(false);
  readonly mostrarModal = signal(false);
  readonly nombreSeleccionado = signal<string>('');

  readonly formulario = this.fb.nonNullable.group({
    usuarioId: [''],
    numeroDocumento: ['', [Validators.required]],
    tipoDocumento: ['CC', [Validators.required]],
    programaAcademico: ['', [Validators.required]],
    semestre: [1, [Validators.required, Validators.min(1), Validators.max(20)]],
  });

  get f() {
    return this.formulario.controls;
  }

  constructor() {
  // Sincroniza el formulario cada vez que cambia el estudiante a editar
  effect(() => {
    const actual = this.estudiante();
    if (actual) {
      this.formulario.setValue({
        usuarioId: actual.usuarioId ?? '',
        numeroDocumento: actual.numeroDocumento,
        tipoDocumento: actual.tipoDocumento,
        programaAcademico: actual.programaAcademico,
        semestre: actual.semestre,
      });
      this.formulario.controls.usuarioId.disable();
      this.cargarNombreEstudiante(actual);
    } else {
      this.formulario.reset({ tipoDocumento: 'CC', semestre: 1 });
      this.formulario.controls.usuarioId.enable();
      this.nombreSeleccionado.set('');
    }
  }, { allowSignalWrites: true });
}

  abrirModal(): void {
    this.mostrarModal.set(true);
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

  const enEdicion = this.estudiante();

  const confirmado = await this.sweetAlert.confirmar(
    enEdicion ? '¿Actualizar estudiante?' : '¿Crear estudiante?',
    enEdicion
      ? 'Se guardarán los cambios realizados.'
      : 'Se registrará un nuevo estudiante con los datos ingresados.',
    { icono: 'question', confirmText: enEdicion ? 'Sí, actualizar' : 'Sí, crear' }
  );
  if (!confirmado) {
    return;
  }

  this.guardando.set(true);
  const valores = this.formulario.getRawValue();

  const payload = {
    ...valores,
    usuarioId: valores.usuarioId ? valores.usuarioId : null,   // 👈 clave
  };

  const peticion = enEdicion
    ? this.estudiantesService.actualizar(enEdicion.id, payload)   // 👈 usa payload, no valores
    : this.estudiantesService.crear(payload);                     // 👈 usa payload, no valores

  peticion.pipe(finalize(() => this.guardando.set(false))).subscribe({
    next: () => {
      this.sweetAlert.exito(
        enEdicion ? 'Estudiante actualizado correctamente.' : 'Estudiante creado correctamente.'
      );
      this.guardado.emit();
    },
    error: (error) => {
      const problema = error?.error as ProblemDetails | undefined;
      const mensaje =
        problema?.detail ||
        problema?.title ||
        (enEdicion ? 'No se pudo actualizar el estudiante.' : 'No se pudo crear el estudiante.');
      this.sweetAlert.error(mensaje);
    },
  });
}

  cancelar(): void {
    this.cancelado.emit();
  }

  private cargarNombreEstudiante(estudiante: Estudiante): void {
  this.nombreSeleccionado.set('Cargando...');
  this.estudiantesService.buscarPorDocumento(estudiante.numeroDocumento).subscribe({
    next: (resultados) => {
      const encontrado =
        resultados.find((r) => r.id === estudiante.id) ?? resultados[0] ?? null;
      this.nombreSeleccionado.set(encontrado?.nombreCompleto ?? '');
    },
    error: () => this.nombreSeleccionado.set(''),
  });
}
}
