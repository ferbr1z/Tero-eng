<script setup lang="ts">
import type { CrudFilterDef } from '~/composables/useCrudFilters'

defineProps<{
  filters: CrudFilterDef[]
  activeCount: number
  isDisabled: (def: CrudFilterDef) => boolean
}>()

const emit = defineEmits<{ clear: [] }>()

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const values = defineModel<Record<string, any>>({ required: true })

function items(def: Extract<CrudFilterDef, { type: 'select' | 'multiselect' }>) {
  return toValue(def.items)
}
</script>

<template>
  <UPopover>
    <UButton
      icon="i-lucide-list-filter"
      color="neutral"
      variant="outline"
    >
      <template v-if="activeCount" #trailing>
        <UBadge
          :label="activeCount"
          color="primary"
          variant="subtle"
          size="sm"
        />
      </template>
    </UButton>

    <template #content>
      <div class="flex flex-col gap-3 p-4 w-72">
        <UButton
          v-if="activeCount"
          label="Limpiar filtros"
          color="error"
          variant="ghost"
          size="sm"
          icon="i-lucide-x"
          @click="emit('clear')"
        />

        <UFormField
          v-for="def in filters"
          :key="def.key"
          :label="def.type === 'boolean' ? undefined : def.label"
        >
          <USelectMenu
            v-if="def.type === 'select'"
            v-model="values[def.key]"
            :items="items(def)"
            :filter-fields="def.filterFields"
            value-key="value"
            class="w-full"
            :placeholder="def.placeholder"
            clear
          />

          <USelectMenu
            v-else-if="def.type === 'multiselect'"
            v-model="values[def.key]"
            :items="items(def)"
            :filter-fields="def.filterFields"
            value-key="value"
            multiple
            class="w-full"
            :placeholder="def.placeholder"
            clear
          />

          <UInput
            v-else-if="def.type === 'text'"
            v-model="values[def.key]"
            :type="def.inputType"
            class="w-full"
            :placeholder="def.placeholder"
          />

          <UInputNumber
            v-else-if="def.type === 'number'"
            v-model="values[def.key]"
            :min="def.min"
            :max="def.max"
            :step="def.step"
            class="w-full"
          />

          <USwitch
            v-else-if="def.type === 'boolean'"
            v-model="values[def.key]"
            :label="def.label"
          />

          <div v-else-if="def.type === 'numberRange'" class="flex items-center gap-2">
            <UFormField
              v-for="bound in (['from', 'to'] as const)"
              :key="bound"
              :label="bound === 'from' ? 'Mín.' : 'Máx.'"
              size="xs"
              class="flex-1"
            >
              <UInputNumber
                v-model="values[def.key][bound]"
                :disabled="isDisabled(def)"
                :min="def.min ?? 0"
                :step="def.step"
                :locale="def.locale"
                :format-options="def.formatOptions"
                class="w-full"
              />
            </UFormField>
          </div>

          <div v-else-if="def.type === 'dateRange'" class="flex items-center gap-2">
            <UFormField
              v-for="bound in (['from', 'to'] as const)"
              :key="bound"
              :label="bound === 'from' ? 'Desde' : 'Hasta'"
              size="xs"
              class="flex-1"
            >
              <UInput
                v-model="values[def.key][bound]"
                :disabled="isDisabled(def)"
                type="date"
                class="w-full"
              />
            </UFormField>
          </div>
        </UFormField>
      </div>
    </template>
  </UPopover>
</template>
