<script setup lang="ts">
import type { RegistroAuditoria } from '~/types'

const props = defineProps<{
  registro: RegistroAuditoria | null
}>()

const open = defineModel<boolean>('open', { default: false })

function parseJson(value: string | undefined): Record<string, unknown> {
  if (!value) return {}
  try {
    return JSON.parse(value)
  } catch {
    return {}
  }
}

const anteriores = computed(() => parseJson(props.registro?.valoresAnteriores))
const nuevos = computed(() => parseJson(props.registro?.valoresNuevos))

const columnas = computed(() => {
  const keys = new Set([...Object.keys(anteriores.value), ...Object.keys(nuevos.value)])
  return Array.from(keys)
})

function formatValor(value: unknown) {
  if (value === undefined) return '—'
  if (value === null) return 'null'
  return String(value)
}

const tipoLabel = computed(() => ({
  Added: 'Creación',
  Modified: 'Modificación',
  Deleted: 'Eliminación'
}[props.registro?.tipoAuditoria ?? 'Modified']))

const tipoColor = computed(() => ({
  Added: 'success' as const,
  Modified: 'warning' as const,
  Deleted: 'error' as const
}[props.registro?.tipoAuditoria ?? 'Modified']))
</script>

<template>
  <UModal
    v-model:open="open"
    :title="`Auditoría · ${registro?.nombreTabla ?? ''}`"
    :ui="{ content: 'sm:max-w-3xl' }"
  >
    <template #body>
      <div v-if="registro" class="space-y-4">
        <div class="flex flex-wrap items-center gap-2">
          <UBadge :color="tipoColor" variant="subtle">
            {{ tipoLabel }}
          </UBadge>
          <UBadge color="neutral" variant="subtle">
            {{ registro.nombreTabla }}
          </UBadge>
          <span class="text-sm text-muted">
            {{ new Date(registro.fecha).toLocaleString('es-PY') }}
          </span>
        </div>

        <div class="grid grid-cols-2 gap-2 text-sm md:grid-cols-3">
          <div>
            <p class="text-muted">
              Usuario
            </p>
            <p class="font-medium">
              {{ registro.nombreUsuario }}
            </p>
          </div>
          <div>
            <p class="text-muted">
              Clave primaria
            </p>
            <p class="font-medium break-all">
              {{ registro.clavePrimaria }}
            </p>
          </div>
        </div>

        <div class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr class="border-b border-default text-left text-muted">
                <th class="py-2 pr-2 font-medium">
                  Columna
                </th>
                <th class="py-2 px-2 font-medium">
                  Valor anterior
                </th>
                <th class="py-2 pl-2 font-medium">
                  Valor nuevo
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="columna in columnas" :key="columna" class="border-b border-default align-top">
                <td class="py-2 pr-2 font-medium">
                  {{ columna }}
                </td>
                <td class="py-2 px-2 break-all text-muted">
                  {{ formatValor(anteriores[columna]) }}
                </td>
                <td class="py-2 pl-2 break-all">
                  {{ formatValor(nuevos[columna]) }}
                </td>
              </tr>
              <tr v-if="!columnas.length">
                <td colspan="3" class="py-4 text-center text-muted">
                  Sin columnas registradas.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div class="flex justify-end">
        <UButton
          label="Cerrar"
          color="neutral"
          variant="subtle"
          @click="open = false"
        />
      </div>
    </template>
  </UModal>
</template>
