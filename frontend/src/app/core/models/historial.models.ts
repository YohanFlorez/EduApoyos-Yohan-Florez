import { EstadoSolicitud } from "./enums";

export interface HistorialEstadoItem {
  estadoAnterior: EstadoSolicitud;
  estadoNuevo: EstadoSolicitud;
  fechaCambio: string;
  usuarioId: string;
  observacion?: string | null;
}
