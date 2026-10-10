<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { CrudFilterDef } from '~/composables/useCrudFilters'
import type { AuditoriaQueryParams } from '~/composables/useAuditoria'
import type { RegistroAuditoria, TipoAuditoria } from '~/types'
import { wholeDay } from '~/composables/useCrudFilters'

// El acceso a la página lo controla middleware/permisos.global.ts (404 si no tiene Auditoria).
const UButton = resolveComponent('UButton')
const UBadge = resolveComponent('UBadge')

const { getRegistrosAuditoria } = useAuditoria()

const tipoOptions = [
  { label: 'Creación', value: 0 },
  { label: 'Modificación', value: 1 },
  { label: 'Eliminación', value: 2 }
]

// Dos buscadores (tabla y usuario) en lugar del buscador único; ver slot #search.
const nombreTabla = ref('')
const nombreUsuario = ref('')

const filters: CrudFilterDef[] = [
  { key: 'tipoAuditoria', type: 'select', label: 'Tipo', items: tipoOptions, placeholder: 'Todos' },
  { key: 'fecha', type: 'dateRange', label: 'Fecha', fromKey: 'desde', toKey: 'hasta', format: wholeDay() }
]

const tipoLabels: Record<TipoAuditoria, string> = {
  Added: 'Creación',
  Modified: 'Modificación',
  Deleted: 'Eliminación'
}
const tipoColors: Record<TipoAuditoria, 'success' | 'warning' | 'error'> = {
  Added: 'success',
  Modified: 'warning',
  Deleted: 'error'
}

const detail = useRowModal<RegistroAuditoria>()

const columns: TableColumn<RegistroAuditoria>[] = [
  {
    accessorKey: 'fecha',
    header: 'Fecha',
    cell: ({ row }) => new Date(row.original.fecha).toLocaleString('es-PY')
  },
  {
    accessorKey: 'nombreUsuario',
    header: 'Usuario'
  },
  {
    accessorKey: 'nombreTabla',
    header: 'Tabla'
  },
  {
    accessorKey: 'tipoAuditoria',
    header: 'Tipo',
    cell: ({ row }) => h(UBadge, {
      label: tipoLabels[row.original.tipoAuditoria],
      color: tipoColors[row.original.tipoAuditoria],
      variant: 'subtle',
      size: 'sm'
    })
  },
  {
    accessorKey: 'clavePrimaria',
    header: 'Clave primaria',
    cell: ({ row }) => row.original.clavePrimaria || '—'
  },
  {
    // Acción única: botón directo en lugar del menú de acciones.
    id: 'actions',
    enableHiding: false,
    cell: ({ row }) => h(
      'div',
      { class: 'text-right' },
      h(UButton, {
        icon: 'i-lucide-eye',
        color: 'neutral',
        variant: 'ghost',
        size: 'sm',
        onClick: () => detail.show(row.original)
      })
    )
  }
]

const { panel } = await useCrudResource<RegistroAuditoria, AuditoriaQueryParams>({
  key: 'auditoria-list',
  fetcher: getRegistrosAuditoria,
  query: () => ({
    nombreTabla: nombreTabla.value || undefined,
    nombreUsuario: nombreUsuario.value || undefined
  }),
  watch: [nombreTabla, nombreUsuario],
  filters,
  columns
})
</script>

<template>
  <CrudPanel id="auditoria" title="Auditoría" v-bind="panel">
    <template #search>
      <div class="flex min-w-0 flex-1 flex-wrap gap-1.5">
        <UInput
          v-model="nombreTabla"
          class="max-w-sm"
          icon="i-lucide-table"
          placeholder="Buscar por tabla..."
        />
        <UInput
          v-model="nombreUsuario"
          class="max-w-sm"
          icon="i-lucide-user"
          placeholder="Buscar por usuario..."
        />
      </div>
    </template>

    <template #modals>
      <AuditoriaDetalleModal v-model:open="detail.open" :registro="detail.row" />
    </template>
  </CrudPanel>
</template>
