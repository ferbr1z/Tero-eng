import type { BaseQueryParams, RegistroAuditoria } from '~/types'

export interface AuditoriaQueryParams extends BaseQueryParams {
  desde?: string
  hasta?: string
  tipoAuditoria?: number
  nombreUsuario?: string
  nombreTabla?: string
}

export function useAuditoria() {
  const crud = useCrud<RegistroAuditoria, never, AuditoriaQueryParams>('auditoria')

  return {
    getRegistrosAuditoria: crud.getAll
  }
}
