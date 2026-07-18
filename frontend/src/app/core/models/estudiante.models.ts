export interface Estudiante {
  id: string;
  usuarioId: string;
  numeroDocumento: string;
  tipoDocumento: string;
  programaAcademico: string;
  semestre: number;
  activo: boolean;
}

export interface CrearEstudianteRequest {
  usuarioId: string;
  numeroDocumento: string;
  tipoDocumento: string;
  programaAcademico: string;
  semestre: number;
}


export interface UsuarioPendiente {
  readonly id: string;
  readonly nombreCompleto: string;
  readonly email: string;
}

export interface EstudianteBusqueda {
  id: string;
  tipoDocumento: string;
  numeroDocumento: string;
  programaAcademico: string;
  nombreCompleto: string | null;
}
