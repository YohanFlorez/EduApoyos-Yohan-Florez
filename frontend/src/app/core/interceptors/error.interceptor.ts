import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';
import { ProblemDetails } from '../models';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        authService.cerrarSesion();
        router.navigate(['/login']);
        toast.error('Su sesión expiró. Inicie sesión nuevamente.');
        return throwError(() => error);
      }

      const problema = error.error as ProblemDetails | undefined;
      const mensaje = problema?.detail || problema?.title || 'Ocurrió un error inesperado. Intente de nuevo.';
      toast.error(mensaje);

      return throwError(() => error);
    }),
  );
};
