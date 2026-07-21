import { EstadoSolicitud, TipoApoyo } from './enums';

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface CambiarEstadoRequest {
  nuevoEstado: EstadoSolicitud;
  observacion?: string | null;
}

export interface FiltroSolicitudes {
  estado?: EstadoSolicitud | null;
  tipoApoyo?: TipoApoyo | null;
  fechaDesde?: string | null;
  fechaHasta?: string | null;
  pageNumber: number;
  pageSize: number;
}
