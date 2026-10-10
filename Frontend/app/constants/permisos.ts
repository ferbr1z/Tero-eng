// Fuente única de verdad de permisos y roles en el frontend.
// Debe mantenerse sincronizado con el backend:
//   - Backend/Authorization/PermisosEnum.cs
//   - Backend/Authorization/RolesEnum.cs
// Al replicar los ids aquí evitamos consultar el catálogo de permisos por GET.

export const PERMISOS = {
  ManageUsuarios: 'ManageUsuarios',
  ManageRoles: 'ManageRoles',
  ManagePermisos: 'ManagePermisos',
  Auditoria: 'Auditoria'
} as const

export type Permiso = typeof PERMISOS[keyof typeof PERMISOS]

// Los roles se seleccionan por nombre; la base genera sus IDs.
export const ROLES = [
  { nombre: 'Admin' },
  { nombre: 'User' }
] as const

export type RolNombre = typeof ROLES[number]['nombre']

export interface PermisoItem {
  value: Permiso
  label: string
}
``
export interface PermisoGrupo {
  name: string
  permisos: PermisoItem[]
}

// Agrupación y etiquetas para render del formulario. El orden aquí define el orden en la UI.
export const PERMISO_GRUPOS: PermisoGrupo[] = [
  {
    name: 'Usuarios y acceso',
    permisos: [
      { value: PERMISOS.ManageUsuarios, label: 'Administrar usuarios' },
      { value: PERMISOS.ManageRoles, label: 'Administrar roles' },
      { value: PERMISOS.ManagePermisos, label: 'Administrar permisos' },
      { value: PERMISOS.Auditoria, label: 'Ver registro de auditoría' }
    ]
  }
]

// Lista plana de todos los permisos, en el mismo orden que los grupos.
export const ALL_PERMISOS: Permiso[] = PERMISO_GRUPOS.flatMap(grupo => grupo.permisos.map(permiso => permiso.value))

// Mapa de ruta base → permiso necesario para acceder.
// Las rutas que no aparecen aquí quedan accesibles para cualquier usuario autenticado.
// Usado tanto para filtrar el sidebar como para el guard de navegación (ver middleware/permisos.global.ts).
export const RUTA_PERMISOS: Partial<Record<string, Permiso>> = {
  '/settings/usuarios': PERMISOS.ManageUsuarios,
  '/auditoria': PERMISOS.Auditoria
}

// Devuelve el permiso requerido para una ruta, matcheando por prefijo (de más específico a menos)
// para que las subrutas (ej. /settings/usuarios/123) hereden el permiso de su base.
export function permisoDeRuta(path: string): Permiso | undefined {
  const base = Object.keys(RUTA_PERMISOS)
    .sort((a, b) => b.length - a.length)
    .find(ruta => path === ruta || path.startsWith(`${ruta}/`))

  return base ? RUTA_PERMISOS[base] : undefined
}
