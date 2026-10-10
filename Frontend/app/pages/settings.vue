<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'

const { canAccessRoute } = usePermisos()
const route = useRoute()

// Las listas necesitan espacio para las tablas; el formulario de usuario,
// para sus columnas de datos y permisos.
const contentWidth = computed(() => {
  if (route.path === '/settings/usuarios') return 'lg:max-w-5xl'
  if (route.path.startsWith('/settings/usuarios/')) return 'lg:max-w-4xl'
  return 'lg:max-w-2xl'
})

const links = [[
/* Pantalla de perfil de ejemplo, sin respaldo en el backend.
{
  label: 'General',
  icon: 'i-lucide-user',
  to: '/settings',
  exact: true
},
*/
  {
    label: 'Usuarios',
    icon: 'i-lucide-users',
    to: '/settings/usuarios'
  }
/* Pantallas de configuración del template sin respaldo en el backend.
{
  label: 'Members',
  icon: 'i-lucide-users',
  to: '/settings/members'
}, {
  label: 'Notifications',
  icon: 'i-lucide-bell',
  to: '/settings/notifications'
}, {
  label: 'Security',
  icon: 'i-lucide-shield',
  to: '/settings/security'
}
*/
], [
/* Enlace a la documentación del template.
{
  label: 'Documentation',
  icon: 'i-lucide-book-open',
  to: 'https://ui.nuxt.com/docs/getting-started/installation/nuxt',
  target: '_blank'
}
*/
]] satisfies NavigationMenuItem[][]

// Oculta las pestañas cuya ruta el usuario no puede ver (ej. Usuarios sin ManageUsuarios).
// El acceso directo por URL lo bloquea igual middleware/permisos.global.ts (404).
const visibleLinks = computed(() => links.map(group =>
  (group as NavigationMenuItem[]).filter(item => !item.to || canAccessRoute(item.to as string))
))
</script>

<template>
  <UDashboardPanel id="settings" :ui="{ body: 'lg:py-12' }">
    <template #header>
      <UDashboardNavbar title="Configuraciones">
        <template #leading>
          <UDashboardSidebarCollapse />
        </template>
      </UDashboardNavbar>

      <UDashboardToolbar>
        <!-- NOTE: The `-mx-1` class is used to align with the `DashboardSidebarCollapse` button here. -->
        <UNavigationMenu :items="visibleLinks" highlight class="-mx-1 flex-1" />
      </UDashboardToolbar>
    </template>

    <template #body>
      <div :class="['flex min-w-0 w-full flex-col gap-4 sm:gap-6 lg:gap-12 mx-auto', contentWidth]">
        <NuxtPage />
      </div>
    </template>
  </UDashboardPanel>
</template>
