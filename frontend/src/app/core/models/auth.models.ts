import { RolUsuario } from './enums';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  nombreCompleto: string;
  email: string;
  password: string;
  rol: RolUsuario;
  numeroDocumento?: string | null;
  tipoDocumento?: string | null;
  programaAcademico?: string | null;
  semestre?: number | null;
}

export interface AuthResponse {
  token: string;
  expiraEn: string;
  usuarioId: string;
  nombreCompleto: string;
  email: string;
  rol: RolUsuario;
}
