import { EstadoSolicitud, TipoApoyo } from './enums';
import { HistorialEstadoItem } from './historial.models';

export interface SolicitudListItem {
  id: string;
  estudianteId: string;
  tipoApoyo: TipoApoyo;
  montoSolicitado: number;
  estado: EstadoSolicitud;
  fechaSolicitud: string;
  fechaActualizacion: string;
}

export interface SolicitudDetalle {
  id: string;
  estudianteId: string;
  tipoApoyo: TipoApoyo;
  montoSolicitado: number;
  descripcion: string;
  estado: EstadoSolicitud;
  fechaSolicitud: string;
  fechaActualizacion: string;
  asesorId?: string | null;
  historial: HistorialEstadoItem[];
}

export interface CrearSolicitudRequest {
  estudianteId: string;
  tipoApoyo: TipoApoyo;
  montoSolicitado: number;
  descripcion: string;
}
export interface FiltroSolicitudes {
  estado?: EstadoSolicitud | null;
  tipoApoyo?: TipoApoyo | null;
  fechaDesde?: string | null;
  fechaHasta?: string | null;
  pageNumber: number;
  pageSize: number;
}


