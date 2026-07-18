import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Perfil, ActualizarPerfilRequest } from '../models/perfil.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PerfilService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/auth/perfil`;

  obtener() {
    return this.http.get<Perfil>(this.baseUrl);
  }

  actualizar(datos: ActualizarPerfilRequest) {
    return this.http.put<Perfil>(this.baseUrl, datos);
  }
}
