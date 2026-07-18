import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { RolUsuario } from '../../../core/models';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-registro',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, SpinnerComponent],
  templateUrl: './registro.component.html',
  styleUrl: '../login/login.component.scss',
})
export class RegistroComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly RolUsuario = RolUsuario;
  readonly cargando = signal(false);
  readonly errorGeneral = signal<string | null>(null);

  readonly formulario = this.fb.nonNullable.group({
    nombreCompleto: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    rol: [RolUsuario.Estudiante, [Validators.required]],
    numeroDocumento: [''],
    tipoDocumento: ['CC'],
    programaAcademico: [''],
    semestre: [1],
  });

  readonly esEstudiante = computed(() => this.formulario.controls.rol.value === RolUsuario.Estudiante);

  constructor() {
    this.formulario.controls.rol.valueChanges.subscribe((rol) => {
      const camposEstudiante = ['numeroDocumento', 'programaAcademico', 'semestre'] as const;
      for (const campo of camposEstudiante) {
        const control = this.formulario.controls[campo];
        if (rol === RolUsuario.Estudiante) {
          control.addValidators(Validators.required);
        } else {
          control.clearValidators();
        }
        control.updateValueAndValidity();
      }
    });
  }

  enviar(): void {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    this.errorGeneral.set(null);
    this.cargando.set(true);

    const valores = this.formulario.getRawValue();

    this.authService
      .registrar({
        ...valores,
        semestre: this.esEstudiante() ? Number(valores.semestre) : null,
        numeroDocumento: this.esEstudiante() ? valores.numeroDocumento : null,
        tipoDocumento: this.esEstudiante() ? valores.tipoDocumento : null,
        programaAcademico: this.esEstudiante() ? valores.programaAcademico : null,
      })
      .pipe(finalize(() => this.cargando.set(false)))
      .subscribe({
        next: (respuesta) => {
          const destino = respuesta.rol === RolUsuario.Asesor ? '/solicitudes' : '/portal';
          this.router.navigate([destino]);
        },
        error: (error) => {
          this.errorGeneral.set(error?.error?.detail ?? 'No fue posible completar el registro.');
        },
      });
  }

  get f() { return this.formulario.controls; }
}
