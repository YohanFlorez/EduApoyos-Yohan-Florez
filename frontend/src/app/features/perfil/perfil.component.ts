import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { PerfilService } from '../../core/services/perfil.service';
import { SweetAlertService } from '../../core/services/sweet alert.service';
import { AuthService } from '../../core/services/auth.service';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-perfil',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, SpinnerComponent],
  templateUrl: './perfil.component.html',
  styleUrl: './perfil.component.scss',
})
export class PerfilComponent {
  private readonly fb = inject(FormBuilder);
  private readonly perfilService = inject(PerfilService);
  private readonly sweetAlert = inject(SweetAlertService);
  private readonly auth = inject(AuthService);

  readonly cargando = signal(true);
  readonly guardando = signal(false);
  readonly rolActual = signal('');
  readonly fechaRegistro = signal<string | null>(null);

  readonly formulario = this.fb.nonNullable.group({
    nombreCompleto: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
  });

  constructor() {
    this.perfilService
      .obtener()
      .pipe(finalize(() => this.cargando.set(false)))
      .subscribe({
        next: (perfil) => {
          this.formulario.patchValue({
            nombreCompleto: perfil.nombreCompleto,
            email: perfil.email,
          });
          this.rolActual.set(perfil.rol);
          this.fechaRegistro.set(perfil.fechaRegistro);
        },
        error: () => this.sweetAlert.error('No se pudo cargar su información de perfil.'),
      });
  }

  get f() {
    return this.formulario.controls;
  }

  async guardar(): Promise<void> {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    const confirmado = await this.sweetAlert.confirmar(
      '¿Guardar cambios en el perfil?',
      'Se actualizará su información personal.',
      { icono: 'question', confirmText: 'Sí, guardar' }
    );
    if (!confirmado) {
      return;
    }

    this.guardando.set(true);
    this.perfilService
      .actualizar(this.formulario.getRawValue())
      .pipe(finalize(() => this.guardando.set(false)))
      .subscribe({
        next: (perfil) => {
          this.sweetAlert.exito('Perfil actualizado correctamente.');
          // this.auth.actualizarNombreUsuarioActual?.(perfil.nombreCompleto);
        },
        error: () => this.sweetAlert.error('No se pudo actualizar el perfil.'),
      });
  }
}
