import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {  PagedResponse } from '../models';
import { CrearEstudianteRequest, Estudiante, EstudianteBusqueda, UsuarioPendiente } from '../models/estudiante.models';
import {  SolicitudListItem } from '../models/solicitud.models';
import { tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class EstudiantesService {
  private readonly baseUrl = `${environment.apiUrl}/estudiantes`;

  constructor(private readonly http: HttpClient) {}

  listar(pageNumber: number, pageSize: number): Observable<PagedResponse<Estudiante>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<PagedResponse<Estudiante>>(this.baseUrl, { params });
  }

  obtenerPropio(): Observable<Estudiante> {

    return this.http.get<Estudiante>(`${this.baseUrl}/me`);
  }

  obtenerPorId(id: string): Observable<Estudiante> {
    return this.http.get<Estudiante>(`${this.baseUrl}/${id}`);
  }

  crear(request: CrearEstudianteRequest): Observable<Estudiante> {
    return this.http.post<Estudiante>(this.baseUrl, request);
  }

  listarSolicitudes(estudianteId: string, pageNumber: number, pageSize: number): Observable<PagedResponse<SolicitudListItem>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<PagedResponse<SolicitudListItem>>(`${this.baseUrl}/${estudianteId}/solicitudes`, { params });
  }

  listarPendientes(busqueda?: string): Observable<UsuarioPendiente[]> {
  let params = new HttpParams();
  if (busqueda)
  {
    params = params.set('filtro', busqueda);
  }
    return this.http.get<UsuarioPendiente[]>(`${this.baseUrl}/pendientes`, { params });
  }
  actualizar(id: string, data: Partial<Estudiante>) {
    return this.http.put<Estudiante>(`${this.baseUrl}/${id}`, data);
  }

  desactivar(id: string) {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  activar(id: string) {
  return this.http.patch<void>(`${this.baseUrl}/${id}/activar`, {});
  }

  buscarPorDocumento(filtro: string) {
  return this.http.get<EstudianteBusqueda[]>(`${this.baseUrl}/buscar`, {
    params: { filtro },
  });
}
}
