import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormControl, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { debounceTime, distinctUntilChanged, finalize, switchMap } from 'rxjs';
import { SolicitudesService } from '../../../core/services/solicitudes.service';
import { EstudiantesService } from '../../../core/services/estudiantes.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { RolUsuario, TIPO_APOYO_LABEL, TipoApoyo } from '../../../core/models';
import { EstudianteBusqueda } from '../../../core/models/estudiante.models';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-formulario-solicitud',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, SpinnerComponent],
  templateUrl: './formulario-solicitud.component.html',
  styleUrl: './formulario-solicitud.component.scss',
})
export class FormularioSolicitudComponent {
  private readonly fb = inject(FormBuilder);
  private readonly solicitudesService = inject(SolicitudesService);
  private readonly estudiantesService = inject(EstudiantesService);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly tipos = Object.values(TipoApoyo).filter((v) => typeof v === 'number') as TipoApoyo[];
  readonly tipoLabel = TIPO_APOYO_LABEL;

  readonly cargandoEstudiante = signal(true);
  readonly enviando = signal(false);
  readonly errorGeneral = signal<string | null>(null);
  readonly esAsesor = this.auth.rolActual() === RolUsuario.Asesor;

  private estudianteId: string | null = null;

  // --- Modal de búsqueda de estudiante (solo asesor) ---
  readonly mostrarModal = signal(false);
  readonly buscando = signal(false);
  readonly resultadosBusqueda = signal<EstudianteBusqueda[]>([]);
  readonly estudianteSeleccionadoLabel = signal<string>('');
  readonly busquedaControl = new FormControl('', { nonNullable: true });

  readonly formulario = this.fb.nonNullable.group({
    tipoApoyo: [TipoApoyo.Beca, [Validators.required]],
    montoSolicitado: [null as number | null, [Validators.required, Validators.min(1)]],
    descripcion: ['', [Validators.required, Validators.minLength(20), Validators.maxLength(1000)]],
    estudianteIdManual: ['', ], // se completa desde el modal cuando es asesor
  });

  constructor() {
    if (this.esAsesor) {
      this.formulario.controls.estudianteIdManual.addValidators(Validators.required);
      this.cargandoEstudiante.set(false);

      this.busquedaControl.valueChanges
        .pipe(
          debounceTime(300),
          distinctUntilChanged(),
          switchMap((texto) => {
            if (!texto || texto.trim().length === 0) {
              this.resultadosBusqueda.set([]);
              return [];
            }
            this.buscando.set(true);
            return this.estudiantesService
              .buscarPorDocumento(texto)
              .pipe(finalize(() => this.buscando.set(false)));
          })
        )
        .subscribe({
          next: (resultados) => this.resultadosBusqueda.set(resultados ?? []),
          error: () => this.resultadosBusqueda.set([]),
        });
    } else {
      this.estudiantesService.obtenerPropio().subscribe({
        next: (estudiante) => {
          this.estudianteId = estudiante.id;
          this.cargandoEstudiante.set(false);
        },
        error: () => {
          this.errorGeneral.set('No fue posible cargar su información de estudiante.');
          this.cargandoEstudiante.set(false);
        },
      });
    }
  }

  abrirModal(): void {
    this.mostrarModal.set(true);
    this.busquedaControl.setValue('', { emitEvent: false });
    this.resultadosBusqueda.set([]);
  }

  cerrarModal(): void {
    this.mostrarModal.set(false);
  }

  seleccionarEstudiante(estudiante: EstudianteBusqueda): void {
    this.formulario.patchValue({ estudianteIdManual: estudiante.id });

    const nombre = estudiante.nombreCompleto || 'Sin nombre registrado';
    this.estudianteSeleccionadoLabel.set(
      `${nombre} — ${estudiante.tipoDocumento} ${estudiante.numeroDocumento}`
    );

    this.mostrarModal.set(false);
  }

  enviar(): void {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    const valores = this.formulario.getRawValue();
    const estudianteId = this.esAsesor ? valores.estudianteIdManual : this.estudianteId;

    if (!estudianteId) {
      this.errorGeneral.set('No fue posible determinar el estudiante de la solicitud.');
      return;
    }

    this.errorGeneral.set(null);
    this.enviando.set(true);

    this.solicitudesService
      .crear({
        estudianteId,
        tipoApoyo: valores.tipoApoyo,
        montoSolicitado: Number(valores.montoSolicitado),
        descripcion: valores.descripcion,
      })
      .pipe(finalize(() => this.enviando.set(false)))
      .subscribe({
        next: (creada) => {
          this.toast.exito('Solicitud creada correctamente.');
          this.router.navigate(['/solicitudes', creada.id]);
        },
        error: (error) => {
          this.errorGeneral.set(error?.error?.detail ?? 'No fue posible crear la solicitud.');
        },
      });
  }

  get f() { return this.formulario.controls; }
}
