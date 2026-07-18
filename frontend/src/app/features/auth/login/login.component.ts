import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { RolUsuario } from '../../../core/models';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-login',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, SpinnerComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly cargando = signal(false);
  readonly errorGeneral = signal<string | null>(null);

  readonly formulario = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  enviar(): void {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    this.errorGeneral.set(null);
    this.cargando.set(true);

    this.authService
      .login(this.formulario.getRawValue())
      .pipe(finalize(() => this.cargando.set(false)))
      .subscribe({
        next: (respuesta) => {
          const destino = respuesta.rol === RolUsuario.Asesor ? '/solicitudes' : '/portal';
          this.router.navigate([destino]);
        },
        error: (error) => {
          this.errorGeneral.set(error?.error?.detail ?? 'Credenciales inválidas. Verifique su correo y contraseña.');
        },
      });
  }

  get email() { return this.formulario.controls.email; }
  get password() { return this.formulario.controls.password; }
}
