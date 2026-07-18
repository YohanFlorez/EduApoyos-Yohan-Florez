import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { debounceTime, distinctUntilChanged, finalize } from 'rxjs';
import { EstudiantesService } from '../../../../core/services/estudiantes.service';
import { UsuarioPendiente } from '../../../../core/models/estudiante.models';

@Component({
  selector: 'app-buscar-usuario-modal',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule],
  templateUrl: './buscar-usuario-modal.component.html',
styleUrl: './buscar-usuario-modal.component.scss',
})
export class BuscarUsuarioModalComponent {
  private readonly estudiantesService = inject(EstudiantesService);
  private readonly destroyRef = inject(DestroyRef);

  readonly visible = input<boolean>(false);

  readonly cerrado = output<void>();
  readonly seleccionado = output<UsuarioPendiente>();

  readonly buscando = signal(false);
  readonly resultados = signal<UsuarioPendiente[]>([]);
  readonly busquedaControl = new FormControl('', { nonNullable: true });

  constructor() {
    effect(() => {
      if (this.visible()) {
        this.busquedaControl.setValue('', { emitEvent: false });
        this.buscar('');
      }
    });


    this.busquedaControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef) // evita fugas si el componente se destruye
      )
      .subscribe((texto) => this.buscar(texto));
  }

  private buscar(texto: string): void {
    this.buscando.set(true);
    this.estudiantesService
      .listarPendientes(texto)
      .pipe(finalize(() => this.buscando.set(false)))
      .subscribe({
        next: (resultados) => this.resultados.set(resultados),
        error: () => this.resultados.set([]),
      });
  }

  seleccionar(usuario: UsuarioPendiente): void {
    this.seleccionado.emit(usuario);
  }

  cerrar(): void {
    this.cerrado.emit();
  }
}
