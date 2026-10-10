export interface LoginRequest {
  email?: string
  telefono?: string
  password: string
}

export interface AuthUser {
  id: number
  email: string
  nombre: string
  isTemporal: boolean
  permisos: string[]
}

export interface LoginResponse extends AuthUser {
  token: string
}

export interface QueryResponse<T> {
  count: number
  page: number
  pageSize: number
  result: T[]
}

export interface BaseQueryParams {
  page?: number
  pageSize?: number
}

export interface Usuario {
  id: number
  nombre: string
  email: string | null
  numDocumento: string
  telefono: string | null
  direccion: string | null
  roles: string[]
}

export interface UsuarioRequest {
  nombre: string
  email?: string
  numDocumento: string
  telefono?: string
  direccion?: string
  roles: string[]
}

export interface UsuarioAddRequest extends UsuarioRequest {
  password: string
}

export interface Rol {
  id: number
  nombre: string
}

export type TipoAuditoria = 'Added' | 'Modified' | 'Deleted'

export interface RegistroAuditoria {
  id: number
  fecha: string
  tipoAuditoria: TipoAuditoria
  idUsuario: number | null
  nombreUsuario: string
  nombreTabla: string
  clavePrimaria: string
  columnasAfectadas: string
  valoresAnteriores: string
  valoresNuevos: string
}
