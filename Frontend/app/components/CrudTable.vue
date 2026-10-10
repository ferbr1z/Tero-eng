<script setup lang="ts" generic="TView">
import type { TableColumn } from '@nuxt/ui'
import type { CrudPanelFilters } from '~/composables/useCrudResource'

// Fragment root: extra attrs from `v-bind="panel"` (e.g. `canCreate`) are intentionally dropped.
defineOptions({ inheritAttrs: false })

defineProps<{
  searchPlaceholder?: string
  data: TView[]
  columns: TableColumn<TView>[]
  total: number
  loading?: boolean
  /** Declarative filters — when set, the toolbar popover + chips render automatically. */
  filters?: CrudPanelFilters
}>()

const search = defineModel<string>('search', { default: '' })
const page = defineModel<number>('page', { default: 1 })
const pageSize = defineModel<number>('pageSize', { default: 10 })

const table = useTemplateRef('table')
const columnVisibility = ref()

defineSlots<{
  /** Replaces the default search box (e.g. a numeric search or several inputs). */
  'search'?: () => unknown
  'filters'?: () => unknown
  'filter-chips'?: () => unknown
}>()
</script>

<template>
  <div class="flex shrink-0 flex-wrap items-center justify-between gap-1.5">
    <slot name="search">
      <UInput
        v-model="search"
        class="min-w-0 flex-1"
        icon="i-lucide-search"
        :placeholder="searchPlaceholder ?? 'Buscar...'"
      />
    </slot>

    <CrudFilters
      v-if="filters"
      :model-value="filters.values"
      :filters="filters.defs"
      :active-count="filters.activeCount"
      :is-disabled="filters.isDisabled"
      @clear="filters.clear"
    />
    <slot v-else name="filters" />

    <UDropdownMenu
      :items="
        table?.tableApi
          ?.getAllColumns()
          .filter((column: any) => column.getCanHide())
          .map((column: any) => ({
            label: column.columnDef.header,
            type: 'checkbox' as const,
            checked: column.getIsVisible(),
            onUpdateChecked(checked: boolean) {
              table?.tableApi?.getColumn(column.id)?.toggleVisibility(checked)
            },
            onSelect(event?: Event) {
              event?.preventDefault()
            }
          }))
      "
      :content="{ align: 'end' }"
    >
      <UButton
        icon="i-lucide-columns-3"
        color="neutral"
        variant="outline"
      />
    </UDropdownMenu>
  </div>

  <div v-if="filters && filters.chips.length" class="flex shrink-0 flex-wrap items-center gap-1.5">
    <UBadge
      v-for="chip in filters.chips"
      :key="chip.key"
      :label="chip.label"
      color="primary"
      variant="subtle"
      class="gap-1 pr-1"
    >
      <template #trailing>
        <UButton
          icon="i-lucide-x"
          color="primary"
          variant="link"
          size="xs"
          :padded="false"
          @click="chip.clear"
        />
      </template>
    </UBadge>
  </div>
  <slot v-else name="filter-chips" />

  <UTable
    ref="table"
    v-model:column-visibility="columnVisibility"
    :data="data"
    :columns="columns"
    :loading="loading"
    sticky="header"
    class="min-h-0 flex-1"
    :ui="{
      base: 'table-fixed border-separate border-spacing-0',
      thead: '[&>tr]:bg-elevated/50 [&>tr]:after:content-none',
      tbody: '[&>tr]:last:[&>td]:border-b-0',
      th: 'py-2 first:rounded-l-lg last:rounded-r-lg border-y border-default first:border-l last:border-r',
      td: 'border-b border-default',
      separator: 'h-0'
    }"
  />

  <div class="flex shrink-0 flex-wrap items-center justify-between gap-3 border-t border-default pt-4">
    <PageSizeSelect v-model="pageSize" />
    <UPagination
      v-model:page="page"
      :items-per-page="pageSize"
      :total="total"
    />
  </div>
</template>
