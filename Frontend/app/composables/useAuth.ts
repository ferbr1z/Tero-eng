import type { AuthUser, LoginRequest, LoginResponse } from '~/types'

export function useAuth() {
  const config = useRuntimeConfig()
  const tokenCookie = useCookie<string | null>('auth_token_v2', { sameSite: 'lax' })
  const userCookie = useCookie<AuthUser | null>('auth_user_v2', { sameSite: 'lax' })

  // Cada useCookie crea un ref propio y entre ellos se sincronizan tarde (o nunca en SSR). El estado
  // compartido (useState) hace que un cambio de sesión se vea al instante en todos los useAuth(),
  // por ejemplo en el reintento de useApi tras una sesión renovada; la cookie solo lo persiste.
  const tokenState = useState<string | null>('auth:token', () => tokenCookie.value ?? null)
  const userState = useState<AuthUser | null>('auth:user', () => userCookie.value ?? null)

  const token = computed<string | null>({
    get: () => tokenState.value,
    set: (value) => {
      tokenState.value = value
      tokenCookie.value = value
    }
  })

  const user = computed<AuthUser | null>({
    get: () => userState.value,
    set: (value) => {
      userState.value = value
      userCookie.value = value
    }
  })

  const isLoggedIn = computed(() => !!token.value)

  // Guarda las credenciales de sesión: las del login o las renovadas que manda el backend cuando
  // cambió la versión de sesión del usuario (ver useApi).
  function applySession(response: LoginResponse) {
    token.value = response.token
    user.value = {
      id: response.id,
      email: response.email,
      nombre: response.nombre,
      isTemporal: response.isTemporal,
      permisos: response.permisos
    }
  }

  function clearSession() {
    token.value = null
    user.value = null
  }

  async function login(payload: LoginRequest) {
    const response = await $fetch<LoginResponse>('/auth/login', {
      baseURL: config.public.apiBase,
      method: 'POST',
      body: payload
    })

    applySession(response)

    return response
  }

  function logout() {
    clearSession()
    return navigateTo('/login')
  }

  return { token, user, isLoggedIn, login, logout, applySession, clearSession }
}
