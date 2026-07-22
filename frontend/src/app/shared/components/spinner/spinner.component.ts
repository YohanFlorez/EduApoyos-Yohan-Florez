import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
  selector: 'app-spinner',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="spinner-wrap" [class.spinner-inline]="inline">
      <span class="spinner"></span>
      @if (etiqueta) {
        <span class="spinner-label">{{ etiqueta }}</span>
      }
    </div>
  `,
  styles: [`
    .spinner-wrap {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 10px;
      padding: 32px 0;
      color: var(--color-ink-soft);
      font-size: 0.85rem;
    }
    .spinner-inline { padding: 0; justify-content: flex-start; }
    .spinner {
      width: 16px;
      height: 16px;
      border-radius: 50%;
      border: 2px solid var(--color-line-strong);
      border-top-color: var(--color-primary);
      animation: girar 0.7s linear infinite;
    }
    @keyframes girar {
      to { transform: rotate(360deg); }
    }
    @media (prefers-reduced-motion: reduce) {
      .spinner { animation-duration: 1.6s; }
    }
  `],
})
export class SpinnerComponent {
  @Input() etiqueta = 'Cargando…';
  @Input() inline = false;
}
