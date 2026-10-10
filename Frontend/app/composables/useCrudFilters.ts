import type { MaybeRefOrGetter } from 'vue'

export interface FilterOption {
  label: string
  value: string | number
  /** Extra fields (e.g. `searchText`) that `filterFields` can search on. */
  [key: string]: unknown
}

export interface RangeValue {
  from?: number | string
  to?: number | string
}

export type FilterValue = string | number | (string | number)[] | boolean | RangeValue | undefined
export type FilterValues = Record<string, FilterValue>

interface BaseFilterDef {
  key: string
  label: string
  /** Override how this filter serializes into backend query params. */
  toQuery?: (value: FilterValue) => Record<string, unknown>
  /** Disable (and skip in query + chips) based on the current values of all filters. */
  disabled?: (values: FilterValues) => boolean
  /**
   * Custom chip label, or `false` to never render a chip for this filter (e.g. when another
   * filter's chip already describes it). Only called while the filter is active.
   */
  chip?: false | ((value: FilterValue, values: FilterValues) => string)
  /** Other filter keys reset together with this one when its chip is cleared. */
  resets?: string[]
}

export interface SelectFilterDef extends BaseFilterDef {
  type: 'select'
  items: MaybeRefOrGetter<FilterOption[]>
  placeholder?: string
  /** Option fields the select searches on (defaults to `label`). */
  filterFields?: string[]
  default?: string | number
}

export interface MultiSelectFilterDef extends BaseFilterDef {
  type: 'multiselect'
  items: MaybeRefOrGetter<FilterOption[]>
  placeholder?: string
  /** Option fields the select searches on (defaults to `label`). */
  filterFields?: string[]
  default?: (string | number)[]
}

export interface TextFilterDef extends BaseFilterDef {
  type: 'text'
  placeholder?: string
  inputType?: 'text' | 'tel' | 'email'
  default?: string
}

export interface NumberFilterDef extends BaseFilterDef {
  type: 'number'
  placeholder?: string
  min?: number
  max?: number
  step?: number
  default?: number
}

export interface BooleanFilterDef extends BaseFilterDef {
  type: 'boolean'
  default?: boolean
}

export interface NumberRangeFilterDef extends BaseFilterDef {
  type: 'numberRange'
  /** Query key for the lower bound. */
  fromKey: string
  /** Query key for the upper bound. */
  toKey: string
  min?: number
  step?: number
  locale?: string
  formatOptions?: Intl.NumberFormatOptions
  /** Formats each bound for the chip label. */
  chipFormat?: (value: number) => string
}

export interface DateRangeFilterDef extends BaseFilterDef {
  type: 'dateRange'
  fromKey: string
  toKey: string
  /** Serializes each `yyyy-mm-dd` bound for the query (see `wholeDay`). Sent as-is when omitted. */
  format?: (date: string, bound: 'from' | 'to') => string
}

/** `dateRange` format that widens the range to whole days: from 00:00:00 to 23:59:59. */
export function wholeDay(options: { utc?: boolean } = {}) {
  return (date: string, bound: 'from' | 'to') => {
    const time = bound === 'from' ? '00:00:00' : '23:59:59'
    return options.utc ? `${date}T${time}.${bound === 'from' ? '000' : '999'}Z` : `${date}T${time}`
  }
}

export type CrudFilterDef
  = | SelectFilterDef
    | MultiSelectFilterDef
    | TextFilterDef
    | NumberFilterDef
    | BooleanFilterDef
    | NumberRangeFilterDef
    | DateRangeFilterDef

export interface FilterChip {
  key: string
  label: string
  clear: () => void
}

function defaultValue(def: CrudFilterDef): FilterValue {
  switch (def.type) {
    case 'select': return def.default
    case 'multiselect': return def.default ?? []
    case 'text': return def.default ?? ''
    case 'number': return def.default
    case 'boolean': return def.default ?? false
    case 'numberRange': return { from: undefined, to: undefined }
    case 'dateRange': return { from: '', to: '' }
  }
}

function isActive(def: CrudFilterDef, value: FilterValue): boolean {
  switch (def.type) {
    case 'select': return value != null && value !== ''
    case 'multiselect': return Array.isArray(value) && value.length > 0
    case 'text': return typeof value === 'string' && value.trim().length > 0
    case 'number': return value != null
    case 'boolean': return value === true
    case 'numberRange':
    case 'dateRange': {
      const range = value as RangeValue
      return (range?.from != null && range.from !== '') || (range?.to != null && range.to !== '')
    }
  }
}

export function useCrudFilters(defs: CrudFilterDef[]) {
  const values = reactive<FilterValues>(
    Object.fromEntries(defs.map(def => [def.key, defaultValue(def)]))
  )

  function isDisabled(def: CrudFilterDef): boolean {
    return def.disabled?.(values) ?? false
  }

  function resolveLabels(def: SelectFilterDef | MultiSelectFilterDef, selected: (string | number)[]): string[] {
    const options = toValue(def.items)
    return selected.map(v => options.find(i => i.value === v)?.label ?? String(v))
  }

  const query = computed<Record<string, unknown>>(() => {
    const result: Record<string, unknown> = {}

    for (const def of defs) {
      if (isDisabled(def) || !isActive(def, values[def.key])) continue

      const value = values[def.key]
      if (def.toQuery) {
        for (const [key, param] of Object.entries(def.toQuery(value))) {
          const current = result[key]
          result[key] = Array.isArray(current) && Array.isArray(param) ? [...current, ...param] : param
        }
        continue
      }

      switch (def.type) {
        case 'numberRange': {
          const range = value as RangeValue
          if (range.from != null) result[def.fromKey] = range.from
          if (range.to != null) result[def.toKey] = range.to
          break
        }
        case 'dateRange': {
          const range = value as RangeValue
          const format = def.format ?? ((date: string) => date)
          if (range.from) result[def.fromKey] = format(String(range.from), 'from')
          if (range.to) result[def.toKey] = format(String(range.to), 'to')
          break
        }
        case 'boolean':
          result[def.key] = true
          break
        default:
          result[def.key] = value
      }
    }

    return result
  })

  function defaultLabel(def: CrudFilterDef, value: FilterValue): string {
    switch (def.type) {
      case 'select':
        return `${def.label}: ${resolveLabels(def, [value as string | number])[0]}`
      case 'multiselect':
        return `${def.label}: ${resolveLabels(def, value as (string | number)[]).join(', ')}`
      case 'text':
      case 'number':
        return `${def.label}: ${value}`
      case 'boolean':
        return def.label
      case 'numberRange': {
        const range = value as RangeValue
        const fmt = def.chipFormat ?? ((n: number) => String(n))
        const min = range.from != null ? fmt(range.from as number) : '0'
        const max = range.to != null ? fmt(range.to as number) : '∞'
        return `${def.label}: ${min} - ${max}`
      }
      case 'dateRange': {
        const range = value as RangeValue
        return `${def.label}: ${range.from || 'inicio'} – ${range.to || 'fin'}`
      }
    }
  }

  function reset(key: string) {
    const def = defs.find(d => d.key === key)
    if (def) values[key] = defaultValue(def)
  }

  const chips = computed<FilterChip[]>(() => {
    const result: FilterChip[] = []

    for (const def of defs) {
      if (def.chip === false || isDisabled(def) || !isActive(def, values[def.key])) continue
      const value = values[def.key]

      result.push({
        key: def.key,
        label: def.chip ? def.chip(value, values) : defaultLabel(def, value),
        clear: () => {
          reset(def.key)
          def.resets?.forEach(reset)
        }
      })
    }

    return result
  })

  const activeCount = computed(() => chips.value.length)

  function clearAll() {
    defs.forEach(def => reset(def.key))
  }

  return { values, query, chips, activeCount, clearAll, isDisabled }
}
