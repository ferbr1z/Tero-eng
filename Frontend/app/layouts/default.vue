<script setup lang="ts">
import type { CommandPaletteItem, NavigationMenuItem } from '@nuxt/ui'

const { canAccessRoute } = usePermisos()
const { token } = useAuth()
const api = useApi()

const open = ref(false)
let sessionCheck: Promise<void> | null = null
let lastSessionCheck = 0

function checkSession() {
  if (!token.value || document.visibilityState === 'hidden' || sessionCheck) return
  if (Date.now() - lastSessionCheck < 1000) return
  lastSessionCheck = Date.now()
  sessionCheck = api('/auth/session').then(() => {}).catch(() => {}).finally(() => {
    sessionCheck = null
  })
}

onMounted(() => {
  lastSessionCheck = Date.now()
  window.addEventListener('focus', checkSession)
  document.addEventListener('visibilitychange', checkSession)
})

onBeforeUnmount(() => {
  window.removeEventListener('focus', checkSession)
  document.removeEventListener('visibilitychange', checkSession)
})

function closeSidebar() {
  open.value = false
}

const sections: { id: string, label: string, items: NavigationMenuItem[] }[] = [
  {
    id: 'general',
    label: 'General',
    items: [
      { label: 'Inicio', icon: 'i-lucide-house', to: '/', onSelect: closeSidebar },
      { label: 'Usuarios', icon: 'i-lucide-users', to: '/settings/usuarios', onSelect: closeSidebar },
      { label: 'Auditoría', icon: 'i-lucide-scroll-text', to: '/auditoria', onSelect: closeSidebar }
    ]
  }
]

const visibleSections = computed(() => sections.map(section => ({
  ...section,
  items: section.items.filter(item => !item.to || canAccessRoute(item.to as string))
})).filter(section => section.items.length > 0))

const sidebarItems = computed<NavigationMenuItem[]>(() => visibleSections.value.flatMap((section, index) => [
  { label: section.label, type: 'label', class: index === 0 ? 'mt-2' : 'mt-3 border-t border-default pt-3' },
  ...section.items
]))

const groups = computed(() => visibleSections.value.map(section => ({
  id: section.id,
  label: section.label,
  items: section.items as CommandPaletteItem[]
})))
</script>

<template>
  <UDashboardGroup unit="rem">
    <UDashboardSidebar
      id="default"
      v-model:open="open"
      collapsible
      resizable
      class="bg-elevated/25"
      :ui="{ footer: 'lg:border-t lg:border-default' }"
    >
      <template #default="{ collapsed }">
        <UDashboardSearchButton :collapsed="collapsed" class="bg-transparent ring-default" />

        <UNavigationMenu
          :collapsed="collapsed"
          :items="sidebarItems"
          orientation="vertical"
          tooltip
          popover
        />
      </template>

      <template #footer="{ collapsed }">
        <UserMenu :collapsed="collapsed" />
      </template>
    </UDashboardSidebar>

    <UDashboardSearch :groups="groups" />
    <slot />
  </UDashboardGroup>
</template>
