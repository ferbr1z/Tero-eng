import type { BaseQueryParams, QueryResponse, Rol, Usuario, UsuarioAddRequest, UsuarioRequest } from '~/types'

export interface UsuariosQueryParams extends BaseQueryParams {
  nombre?: string
  email?: string
  telefono?: string
  numDocumento?: string
  roleIds?: number[]
}

export function useUsuarios() {
  const api = useApi()
  const crud = useCrud<Usuario, UsuarioRequest, UsuariosQueryParams>('usuarios')

  // create diverge del update (UsuarioAddRequest incluye password), va directo sobre useApi
  function createUsuario(body: UsuarioAddRequest) {
    return api<Usuario>('/usuarios', { method: 'POST', body })
  }

  function getRoles() {
    return api<QueryResponse<Rol>>('/roles', { params: { pageSize: 100 } })
  }

  function getPermisos(id: number) {
    return api<string[]>(`/permisos/${id}`)
  }

  function getPermisosRol(nombre: string) {
    return api<string[]>(`/permisos/rol/nombre/${encodeURIComponent(nombre)}`)
  }

  function setPermisos(id: number, permisos: Record<string, boolean>) {
    return api<unknown>(`/permisos/${id}`, { method: 'POST', body: { permisos } })
  }

  return {
    getUsuarios: crud.getAll,
    getUsuario: crud.getById,
    createUsuario,
    updateUsuario: crud.update,
    deleteUsuario: crud.remove,
    getRoles,
    getPermisos,
    getPermisosRol,
    setPermisos
  }
}
