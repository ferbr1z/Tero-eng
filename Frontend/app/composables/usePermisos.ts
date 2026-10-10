import { ALL_PERMISOS, PERMISO_GRUPOS, PERMISOS, permisoDeRuta } from '~/constants/permisos'
import type { Permiso } from '~/constants/permisos'

// Hook central de autorización en el frontend.
// Compara los permisos que el usuario recibió en el login (guardados en la sesión)
// contra el catálogo declarado en ~/constants/permisos.ts, y expone helpers reactivos
// para decidir qué puede ver o hacer el usuario, sin volver a consultar al backend.
export function usePermisos() {
  const { user } = useAuth()

  // Set reactivo con los permisos del usuario actual.
  const permisosUsuario = computed(() => new Set(user.value?.permisos ?? []))

  function has(permiso: Permiso): boolean {
    return permisosUsuario.value.has(permiso)
  }

  function hasAny(...permisos: Permiso[]): boolean {
    return permisos.some(permiso => permisosUsuario.value.has(permiso))
  }

  function hasAll(...permisos: Permiso[]): boolean {
    return permisos.every(permiso => permisosUsuario.value.has(permiso))
  }

  // Permisos del catálogo que el usuario efectivamente tiene (ignora ids desconocidos del backend).
  const permisosConcedidos = computed(() => ALL_PERMISOS.filter(permiso => permisosUsuario.value.has(permiso)))

  // Grupos del catálogo filtrados a lo que el usuario puede ver; útil para armar menús/secciones.
  const gruposConcedidos = computed(() =>
    PERMISO_GRUPOS
      .map(grupo => ({ ...grupo, permisos: grupo.permisos.filter(permiso => permisosUsuario.value.has(permiso.value)) }))
      .filter(grupo => grupo.permisos.length > 0)
  )

  // ¿El usuario puede acceder a esta ruta? Las rutas sin permiso declarado son siempre accesibles.
  function canAccessRoute(path: string): boolean {
    const permiso = permisoDeRuta(path)
    return !permiso || permisosUsuario.value.has(permiso)
  }

  return { PERMISOS, has, hasAny, hasAll, permisosConcedidos, gruposConcedidos, canAccessRoute }
}
