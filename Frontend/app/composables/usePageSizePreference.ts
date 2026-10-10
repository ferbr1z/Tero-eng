const DEFAULT_PAGE_SIZE = 10
const ONE_YEAR_IN_SECONDS = 60 * 60 * 24 * 365

/**
 * Shared rows-per-page preference, scoped to the authenticated user.
 * The default value is not stored: selecting it removes the existing cookie.
 */
export function usePageSizePreference(defaultPageSize = DEFAULT_PAGE_SIZE) {
  const { user } = useAuth()
  const userKey = user.value?.id ?? 'anonymous'
  const preference = useCookie<number | null>(`page_size_${userKey}`, {
    default: () => null,
    maxAge: ONE_YEAR_IN_SECONDS,
    sameSite: 'lax'
  })

  return computed<number>({
    get: () => {
      const value = preference.value
      return typeof value === 'number' && Number.isFinite(value) && value > 0
        ? value
        : defaultPageSize
    },
    set: (value) => {
      preference.value = value === defaultPageSize ? null : value
    }
  })
}
