export enum RolUsuario {
  Asesor = 1,
  Estudiante = 2,
}

export enum TipoApoyo {
  Beca = 1,
  Credito = 2,
  Subsidio = 3,
}

export enum EstadoSolicitud {
  Pendiente = 1,
  EnRevision = 2,
  Aprobada = 3,
  Rechazada = 4,
}

export const TIPO_APOYO_LABEL: Record<TipoApoyo, string> = {
  [TipoApoyo.Beca]: 'Beca',
  [TipoApoyo.Credito]: 'Crédito',
  [TipoApoyo.Subsidio]: 'Subsidio',
};

export const ESTADO_LABEL: Record<EstadoSolicitud, string> = {
  [EstadoSolicitud.Pendiente]: 'Pendiente',
  [EstadoSolicitud.EnRevision]: 'En revisión',
  [EstadoSolicitud.Aprobada]: 'Aprobada',
  [EstadoSolicitud.Rechazada]: 'Rechazada',
};


export const TRANSICIONES_VALIDAS: Record<EstadoSolicitud, EstadoSolicitud[]> = {
  [EstadoSolicitud.Pendiente]: [EstadoSolicitud.EnRevision, EstadoSolicitud.Rechazada],
  [EstadoSolicitud.EnRevision]: [EstadoSolicitud.Aprobada, EstadoSolicitud.Rechazada],
  [EstadoSolicitud.Aprobada]: [],
  [EstadoSolicitud.Rechazada]: [],
};
