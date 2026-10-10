<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { CrudFilterDef } from '~/composables/useCrudFilters'
import type { UsuariosQueryParams } from '~/composables/useUsuarios'
import type { Usuario } from '~/types'

const { getUsuarios, deleteUsuario, getRoles } = useUsuarios()
const toast = useToast()
const { PERMISOS } = usePermisos()

const { data: rolesData } = await useAsyncData('usuarios-roles', () => getRoles())
const roleOptions = computed(() => (rolesData.value?.result ?? []).map(rol => ({ label: rol.nombre, value: rol.id })))

const filters: CrudFilterDef[] = [
  { key: 'roleIds', type: 'multiselect', label: 'Rol', items: roleOptions, placeholder: 'Todos los roles' }
]

const columns: TableColumn<Usuario>[] = [
  { accessorKey: 'nombre', header: 'Nombre' },
  { accessorKey: 'email', header: 'Email', cell: ({ row }) => row.original.email || '—' },
  { accessorKey: 'telefono', header: 'Teléfono', cell: ({ row }) => row.original.telefono || '—' },
  { accessorKey: 'roles', header: 'Rol', cell: ({ row }) => row.original.roles.join(', ') || '—' }
]

async function removeUser(user: Usuario) {
  if (!confirm(`¿Eliminar a ${user.nombre}?`)) return
  try {
    await deleteUsuario(user.id)
    toast.add({ title: 'Usuario eliminado', color: 'success' })
    await refresh()
  } catch (error) {
    toast.add({ title: 'No se pudo eliminar el usuario', description: getApiErrorMessage(error), color: 'error' })
  }
}

const { panel, canCreate, refresh } = await useCrudResource<Usuario, UsuariosQueryParams>({
  key: 'usuarios-list',
  fetcher: getUsuarios,
  searchParam: 'nombre',
  filters,
  permisos: { manage: PERMISOS.ManageUsuarios },
  columns,
  // Editar es una página propia y eliminar confirma en línea, así que no hay modales.
  onEdit: row => navigateTo(`/settings/usuarios/${row.id}`),
  onDelete: removeUser
})
</script>

<template>
  <div class="min-w-0 space-y-4">
    <UPageCard
      title="Usuarios"
      description="Administra los accesos, roles y permisos del equipo."
      variant="naked"
      orientation="horizontal"
    >
      <UButton
        v-if="canCreate"
        label="Nuevo usuario"
        icon="i-lucide-plus"
        to="/settings/usuarios/nuevo"
        class="w-fit lg:ms-auto"
      />
    </UPageCard>

    <UPageCard variant="subtle" class="min-w-0" :ui="{ container: 'min-w-0 gap-y-4', wrapper: 'min-w-0 items-stretch' }">
      <CrudTable search-placeholder="Buscar usuarios..." v-bind="panel" />
    </UPageCard>
  </div>
</template>
