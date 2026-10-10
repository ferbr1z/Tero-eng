<script setup lang="ts" generic="TView">
import type { TableColumn } from '@nuxt/ui'
import type { CrudPanelFilters } from '~/composables/useCrudResource'

// Dashboard page shell for a CRUD list: navbar (title + actions) around a `CrudTable`.
// Pages bind `useCrudResource`'s `panel` in one go: `<CrudPanel v-bind="panel">`.
defineProps<{
  id: string
  title: string
  canCreate?: boolean
  searchPlaceholder?: string
  data: TView[]
  columns: TableColumn<TView>[]
  total: number
  loading?: boolean
  filters?: CrudPanelFilters
}>()

const search = defineModel<string>('search', { default: '' })
const page = defineModel<number>('page', { default: 1 })
const pageSize = defineModel<number>('pageSize', { default: 10 })

defineSlots<{
  /** Replaces the default title. */
  'title'?: () => unknown
  'navbar-actions'?: () => unknown
  /** Rendered only when `canCreate`. */
  'create'?: () => unknown
  'search'?: () => unknown
  'filters'?: () => unknown
  'filter-chips'?: () => unknown
  'modals'?: () => unknown
}>()
</script>

<template>
  <UDashboardPanel :id="id" :ui="{ body: 'min-h-0 overflow-hidden' }">
    <template #header>
      <UDashboardNavbar>
        <template #title>
          <slot name="title">
            {{ title }}
          </slot>
        </template>

        <template #leading>
          <UDashboardSidebarCollapse />
        </template>

        <template #right>
          <slot name="navbar-actions" />
          <slot v-if="canCreate" name="create" />
        </template>
      </UDashboardNavbar>
    </template>

    <template #body>
      <CrudTable
        v-model:search="search"
        v-model:page="page"
        v-model:page-size="pageSize"
        :search-placeholder="searchPlaceholder"
        :data="data"
        :columns="columns"
        :total="total"
        :loading="loading"
        :filters="filters"
      >
        <template v-if="$slots.search" #search>
          <slot name="search" />
        </template>
        <template v-if="$slots.filters" #filters>
          <slot name="filters" />
        </template>
        <template v-if="$slots['filter-chips']" #filter-chips>
          <slot name="filter-chips" />
        </template>
      </CrudTable>
    </template>
  </UDashboardPanel>

  <slot name="modals" />
</template>
