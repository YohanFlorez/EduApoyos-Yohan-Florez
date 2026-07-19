import { Injectable, signal } from '@angular/core';

export type ToastTipo = 'exito' | 'error' | 'info';

export interface Toast {
  id: number;
  tipo: ToastTipo;
  mensaje: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private siguienteId = 1;
  readonly toasts = signal<Toast[]>([]);

  exito(mensaje: string): void {
    this.mostrar('exito', mensaje);
  }

  error(mensaje: string): void {
    this.mostrar('error', mensaje);
  }

  info(mensaje: string): void {
    this.mostrar('info', mensaje);
  }

  descartar(id: number): void {
    this.toasts.update((actuales) => actuales.filter((t) => t.id !== id));
  }

  private mostrar(tipo: ToastTipo, mensaje: string): void {
    const id = this.siguienteId++;
    this.toasts.update((actuales) => [...actuales, { id, tipo, mensaje }]);
    setTimeout(() => this.descartar(id), 5000);
  }
}
