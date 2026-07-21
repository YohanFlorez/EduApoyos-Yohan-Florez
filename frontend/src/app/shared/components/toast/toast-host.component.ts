import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast-host',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="toast-host">
      @for (toast of toastService.toasts(); track toast.id) {
        <div class="toast" [class]="'toast-' + toast.tipo" (click)="toastService.descartar(toast.id)">
          {{ toast.mensaje }}
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-host {
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 1000;
      display: flex;
      flex-direction: column;
      gap: 10px;
      max-width: 360px;
    }
    .toast {
      padding: 12px 16px;
      border-radius: var(--radius-sm);
      font-size: 0.86rem;
      font-weight: 500;
      box-shadow: var(--shadow-raised);
      cursor: pointer;
      animation: entrar 0.18s ease-out;
      border: 1px solid transparent;
    }
    @keyframes entrar {
      from { opacity: 0; transform: translateY(-6px); }
      to { opacity: 1; transform: translateY(0); }
    }
    .toast-exito { background: var(--color-aprobada-bg); color: var(--color-aprobada); border-color: var(--color-aprobada); }
    .toast-error { background: var(--color-rechazada-bg); color: var(--color-rechazada); border-color: var(--color-rechazada); }
    .toast-info  { background: var(--color-revision-bg); color: var(--color-revision); border-color: var(--color-revision); }
  `],
})
export class ToastHostComponent {
  readonly toastService = inject(ToastService);
}
