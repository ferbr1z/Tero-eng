// Guard de autorización por ruta. Corre después de auth.global.ts.
// Si el usuario intenta entrar a una ruta para la que no tiene permiso,
// respondemos con un 404 (renderiza error.vue) en vez de revelar que la página existe.
export default defineNuxtRouteMiddleware((to) => {
  const { isLoggedIn } = useAuth()

  // Usuario no autenticado: lo maneja auth.global.ts (redirige a /login).
  if (!isLoggedIn.value) return

  const { canAccessRoute } = usePermisos()

  if (!canAccessRoute(to.path)) {
    throw createError({ statusCode: 404, statusMessage: 'Página no encontrada' })
  }
})
