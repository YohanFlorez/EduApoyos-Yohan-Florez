import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest, RolUsuario } from '../models';

const STORAGE_KEY = 'eduapoyos_sesion';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly sesion = signal<AuthResponse | null>(this.leerSesionGuardada());

  readonly usuarioActual = computed(() => this.sesion());
  readonly estaAutenticado = computed(() => !!this.sesion());
  readonly rolActual = computed<RolUsuario | null>(() => this.sesion()?.rol ?? null);

  constructor(private readonly http: HttpClient) {}

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/login`, request)
      .pipe(tap((respuesta) => this.guardarSesion(respuesta)));
  }

  registrar(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/register`, request)
      .pipe(tap((respuesta) => this.guardarSesion(respuesta)));
  }

  cerrarSesion(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.sesion.set(null);
  }

  obtenerToken(): string | null {
    return this.sesion()?.token ?? null;
  }

  private guardarSesion(respuesta: AuthResponse): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(respuesta));
    this.sesion.set(respuesta);
  }

  private leerSesionGuardada(): AuthResponse | null {
    const crudo = localStorage.getItem(STORAGE_KEY);
    if (!crudo) return null;
    try {
      const sesion = JSON.parse(crudo) as AuthResponse;
      if (new Date(sesion.expiraEn).getTime() <= Date.now()) {
        localStorage.removeItem(STORAGE_KEY);
        return null;
      }
      return sesion;
    } catch {
      return null;
    }
  }
}
