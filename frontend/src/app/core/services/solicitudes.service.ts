import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CambiarEstadoRequest,
  PagedResponse,
} from '../models';

import {
  CrearSolicitudRequest,
  FiltroSolicitudes,
  SolicitudDetalle,
  SolicitudListItem,
} from '../models/solicitud.models';

@Injectable({ providedIn: 'root' })
export class SolicitudesService {
  private readonly baseUrl = `${environment.apiUrl}/solicitudes`;

  constructor(private readonly http: HttpClient) {}

  listar(filtro: FiltroSolicitudes): Observable<PagedResponse<SolicitudListItem>> {
    let params = new HttpParams().set('pageNumber', filtro.pageNumber).set('pageSize', filtro.pageSize);
    if (filtro.estado != null) params = params.set('estado', filtro.estado);
    if (filtro.tipoApoyo != null) params = params.set('tipoApoyo', filtro.tipoApoyo);
    if (filtro.fechaDesde) params = params.set('fechaDesde', filtro.fechaDesde);
    if (filtro.fechaHasta) params = params.set('fechaHasta', filtro.fechaHasta);

    return this.http.get<PagedResponse<SolicitudListItem>>(this.baseUrl, { params });
  }

  obtenerDetalle(id: string): Observable<SolicitudDetalle> {
    return this.http.get<SolicitudDetalle>(`${this.baseUrl}/${id}`);
  }

  crear(request: CrearSolicitudRequest): Observable<SolicitudDetalle> {
    return this.http.post<SolicitudDetalle>(this.baseUrl, request);
  }

  cambiarEstado(id: string, request: CambiarEstadoRequest): Observable<SolicitudDetalle> {
    return this.http.patch<SolicitudDetalle>(`${this.baseUrl}/${id}/estado`, request);
  }
}
