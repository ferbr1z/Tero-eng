import type { LoginResponse } from '~/types'

const SESSION_RENEWED_HEADER = 'X-Session-Renewed'
const MAX_SESSION_RENEWALS = 3

export function useApi() {
  const config = useRuntimeConfig()
  const { token, user, applySession, clearSession } = useAuth()
  const fetcher = $fetch.create({ baseURL: config.public.apiBase })

  type Request = Parameters<typeof fetcher>[0]
  type Options = Parameters<typeof fetcher>[1]

  return async function api<T = unknown>(request: Request, options?: Options): Promise<T> {
    for (let renewals = 0; ; renewals++) {
      // Cada respuesta solo puede cambiar la sesión que efectivamente envió este intento.
      const requestToken = token.value
      const requestUserId = user.value?.id
      const headers = new Headers(options?.headers)
      if (requestToken) headers.set('Authorization', `Bearer ${requestToken}`)
      else headers.delete('Authorization')

      try {
        return await (fetcher(request, { ...options, headers }) as Promise<T>)
      } catch (error) {
        const { response } = (error ?? {}) as { response?: Response }
        const session = getRenewedSession(error)
        if (session) {
          // Otro request ya renovó el token, o el usuario cerró sesión mientras este esperaba.
          if (token.value !== requestToken) {
            if (!token.value || user.value?.id !== requestUserId) throw error
          } else {
            applySession(session)
          }
          if (renewals >= MAX_SESSION_RENEWALS) throw error
          continue
        }

        if (response?.status === 401 && token.value === requestToken) {
          clearSession()
          if (import.meta.client) navigateTo('/login')
        }
        throw error
      }
    }
  }
}

function getRenewedSession(error: unknown): LoginResponse | null {
  const { response, data } = (error ?? {}) as { response?: Response, data?: unknown }
  if (!response?.headers.get(SESSION_RENEWED_HEADER)) return null
  return data as LoginResponse
}
