export interface Perfil {
  id: string;
  nombreCompleto: string;
  email: string;
  rol: string;
  fechaRegistro: string;
}

export interface ActualizarPerfilRequest {
  nombreCompleto: string;
  email: string;
}
