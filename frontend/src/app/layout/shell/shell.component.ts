import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { RolUsuario } from '../../core/models';
import { ToastHostComponent } from '../../shared/components/toast/toast-host.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, ToastHostComponent],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
})
export class ShellComponent {
  readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  esAsesor(): boolean {
    return this.auth.rolActual() === RolUsuario.Asesor;
  }

  salir(): void {
    this.auth.cerrarSesion();
    this.toast.info('Sesión cerrada.');
    this.router.navigate(['/login']);
  }


  readonly menuAbierto = signal(false);

  toggleMenu(): void {
    this.menuAbierto.update((v) => !v);
  }

  cerrarMenu(): void {
    this.menuAbierto.set(false);
  }


}
