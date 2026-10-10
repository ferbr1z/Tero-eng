import type { BaseQueryParams, QueryResponse } from '~/types'

export interface CrudApi<TView, TRequest, TQuery extends BaseQueryParams> {
  getAll: (query?: TQuery) => Promise<QueryResponse<TView>>
  getById: (id: number) => Promise<TView>
  create: (body: TRequest) => Promise<TView>
  update: (id: number, body: TRequest) => Promise<TView>
  remove: (id: number) => Promise<TView>
}

export function useCrud<TView, TRequest = TView, TQuery extends BaseQueryParams = BaseQueryParams>(
  resource: string
): CrudApi<TView, TRequest, TQuery> {
  const api = useApi()
  const base = `/${resource}`

  return {
    getAll: query => api<QueryResponse<TView>>(base, { params: query }),
    getById: id => api(`${base}/${id}`) as Promise<TView>,
    create: body => api(base, { method: 'POST', body: body as Record<string, unknown> }) as Promise<TView>,
    update: (id, body) => api(`${base}/${id}`, { method: 'PUT', body: body as Record<string, unknown> }) as Promise<TView>,
    remove: id => api(`${base}/${id}`, { method: 'DELETE' }) as Promise<TView>
  }
}
