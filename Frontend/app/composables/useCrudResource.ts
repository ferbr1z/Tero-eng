import type { MaybeRefOrGetter, WatchSource } from 'vue'
import type { DropdownMenuItem, TableColumn } from '@nuxt/ui'
import type { Permiso } from '~/constants/permisos'
import type { CrudFilterDef, FilterChip, FilterValues } from '~/composables/useCrudFilters'
import type { BaseQueryParams, QueryResponse } from '~/types'
// Explicit imports: Nuxt only compiles `resolveComponent('UButton')` into an import inside .vue
// files; in a .ts composable it would resolve at runtime, fail, and render an empty element.
import { UButton, UDropdownMenu } from '#components'

/** A permission id, or a plain boolean for actions that aren't permission-gated. */
type CrudPermiso = Permiso | boolean

type CrudPermisos
  = | { create?: CrudPermiso, update?: CrudPermiso, delete?: CrudPermiso, manage?: undefined }
    | { manage: CrudPermiso, create?: undefined, update?: undefined, delete?: undefined }

/** Everything `CrudPanel` needs to render the filter popover + chips, grouped into one prop. */
export interface CrudPanelFilters {
  defs: CrudFilterDef[]
  values: FilterValues
  chips: FilterChip[]
  activeCount: number
  isDisabled: (def: CrudFilterDef) => boolean
  clear: () => void
}

/** Open/row state for a modal that acts on a single row (edit, delete, detail, ...). */
export interface RowModal<T> {
  open: boolean
  row: T | null
  show: (row: T) => void
}

/** Reactive so templates can bind `v-model:open="modal.open"` / `:item="modal.row"` directly. */
export function useRowModal<T>(): RowModal<T> {
  const modal = reactive({
    open: false,
    row: null as T | null,
    show(row: T) {
      modal.row = row as never
      modal.open = true
    }
  })
  return modal as RowModal<T>
}

type RowAction = DropdownMenuItem | false | null | undefined

export interface UseCrudResourceOptions<TView, TQuery extends BaseQueryParams> {
  /** Must be globally unique — it is the `useAsyncData` cache key for this list. */
  key: string
  fetcher: (query: TQuery) => Promise<QueryResponse<TView>>
  columns: MaybeRefOrGetter<TableColumn<TView>[]>
  permisos?: CrudPermisos
  /** Query param that receives the search box text (e.g. `'name'`). Omit to ignore the search box. */
  searchParam?: keyof TQuery & string
  /**
   * Extra query params, merged under filters/search/paging. Receives the search box text for
   * cases `searchParam` can't express (e.g. parsing it as a number). Refs read here must also
   * be listed in `watch`.
   */
  query?: (ctx: { search: string }) => Partial<Omit<TQuery, 'page' | 'pageSize'>>
  /** Declarative filters rendered in the toolbar popover; also produce chips + query params. */
  filters?: CrudFilterDef[]
  /**
   * Row menu items shown before Editar/Eliminar. Falsy entries are skipped, so conditional
   * items can be written inline: `row.status === 'Completed' && { label: 'Anular', ... }`.
   */
  rowActions?: (row: TView) => RowAction[]
  /** Replace the default "open the `edit` modal" behaviour (e.g. navigate to an edit page). */
  onEdit?: (row: TView) => void
  /** Replace the default "open the `remove` modal" behaviour. */
  onDelete?: (row: TView) => void
  /** Extra sources that trigger a refetch (and reset to page 1). */
  watch?: WatchSource[]
  pageSize?: number
}

/**
 * List page state for a paginated, filterable CRUD table. Pair with `<CrudPanel v-bind="panel">`
 * (dashboard pages) or `<CrudTable v-bind="panel">` (inside another layout, e.g. settings).
 * Async (awaits the first fetch) so the list is server-rendered like a plain `await useAsyncData`.
 */
export async function useCrudResource<TView, TQuery extends BaseQueryParams = BaseQueryParams>(
  options: UseCrudResourceOptions<TView, TQuery>
) {
  const { has } = usePermisos()
  const permisos = options.permisos ?? {}

  const search = ref('')
  const page = ref(1)
  const pageSize = usePageSizePreference(options.pageSize)
  const watchSources = options.watch ?? []

  const filterDefs = options.filters ?? []
  const filters = useCrudFilters(filterDefs)

  const asyncData = useAsyncData(
    options.key,
    () => options.fetcher({
      ...options.query?.({ search: search.value }),
      ...filters.query.value,
      ...(options.searchParam && search.value ? { [options.searchParam]: search.value } : {}),
      page: page.value - 1,
      pageSize: pageSize.value
    } as TQuery),
    { watch: [search, filters.query, ...watchSources, page, pageSize] }
  )
  const { data, status, refresh } = asyncData

  // Reset to page 1 when a non-page source changes. `flush: 'sync'` runs this before
  // useAsyncData's (pre-flush) watcher, so search+page changes batch into a single fetch
  // instead of firing one request with the stale page and a second with page 1.
  watch([search, pageSize, filters.query, ...watchSources], () => {
    page.value = 1
  }, { flush: 'sync' })

  const items = computed(() => data.value?.result ?? [])
  const total = computed(() => data.value?.count ?? 0)

  const allowed = (permiso?: CrudPermiso) => computed(() => {
    const required = permisos.manage ?? permiso
    return typeof required === 'string' ? has(required) : !!required
  })
  const canCreate = allowed(permisos.create)
  const canUpdate = allowed(permisos.update)
  const canDelete = allowed(permisos.delete)

  const edit = useRowModal<TView>()
  const remove = useRowModal<TView>()

  function getRowItems(row: TView): DropdownMenuItem[] {
    // Each group is separated from the next; empty groups are dropped.
    const groups: DropdownMenuItem[][] = [
      (options.rowActions?.(row) ?? []).filter((item): item is DropdownMenuItem => !!item),
      canUpdate.value ? [{ label: 'Editar', icon: 'i-lucide-pencil', onSelect: () => (options.onEdit ?? edit.show)(row) }] : [],
      canDelete.value ? [{ label: 'Eliminar', icon: 'i-lucide-trash', color: 'error', onSelect: () => (options.onDelete ?? remove.show)(row) }] : []
    ]

    return groups
      .filter(group => group.length)
      .reduce<DropdownMenuItem[]>(
        (acc, group, i) => [...acc, ...(i ? [{ type: 'separator' as const }] : []), ...group],
        [{ type: 'label', label: 'Acciones' }]
      )
  }

  const actionsColumn: TableColumn<TView> = {
    id: 'actions',
    enableHiding: false,
    cell: ({ row }) => {
      const rowItems = getRowItems(row.original)
      if (rowItems.length <= 1) return null

      return h(
        'div',
        { class: 'text-right' },
        h(
          UDropdownMenu,
          { content: { align: 'end' }, items: rowItems },
          () => h(UButton, { icon: 'i-lucide-ellipsis-vertical', color: 'neutral', variant: 'ghost', class: 'ml-auto' })
        )
      )
    }
  }

  const hasRowActions = computed(() => !!options.rowActions || canUpdate.value || canDelete.value)
  const columns = computed<TableColumn<TView>[]>(() => {
    const base = toValue(options.columns)
    return hasRowActions.value ? [...base, actionsColumn] : base
  })

  // `reactive` auto-unwraps the nested computed refs (chips/activeCount) when read in the template.
  const filterGroup = filterDefs.length
    ? reactive({
      defs: filterDefs,
      values: filters.values,
      chips: filters.chips,
      activeCount: filters.activeCount,
      isDisabled: filters.isDisabled,
      clear: filters.clearAll
    }) as CrudPanelFilters
    : undefined

  // Props + v-model listeners for `<CrudPanel/CrudTable v-bind="panel">`, so pages don't wire them one by one.
  const panel = reactive({
    'data': items,
    'columns': columns,
    'total': total,
    'loading': computed(() => status.value === 'pending'),
    'canCreate': canCreate,
    'filters': filterGroup,
    'search': search,
    'page': page,
    'pageSize': pageSize,
    'onUpdate:search': (value: string) => { search.value = value },
    'onUpdate:page': (value: number) => { page.value = value },
    'onUpdate:pageSize': (value: number) => { pageSize.value = value }
  })

  await asyncData

  return {
    panel,
    // list state
    search,
    page,
    pageSize,
    items,
    total,
    status,
    refresh,
    filterValues: filters.values,
    // permisos
    canCreate,
    canUpdate,
    canDelete,
    // edit/delete modal state
    edit,
    remove
  }
}
