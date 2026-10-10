<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'

defineProps<{ collapsed?: boolean }>()

const colorMode = useColorMode()
const { user, logout } = useAuth()

const account = computed(() => ({
  name: user.value?.nombre ?? user.value?.email ?? 'Usuario',
  avatar: { alt: user.value?.nombre ?? user.value?.email ?? 'Usuario' }
}))

const items = computed<DropdownMenuItem[][]>(() => [[{
  type: 'label',
  label: account.value.name,
  avatar: account.value.avatar
}], [{
  label: 'Usuarios',
  icon: 'i-lucide-users',
  to: '/settings/usuarios'
}, {
  label: 'Seguridad',
  icon: 'i-lucide-shield',
  to: '/settings/security'
}, {
  label: colorMode.value === 'dark' ? 'Tema claro' : 'Tema oscuro',
  icon: colorMode.value === 'dark' ? 'i-lucide-sun' : 'i-lucide-moon',
  onSelect: () => {
    colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
  }
}], [{
  label: 'Cerrar sesión',
  icon: 'i-lucide-log-out',
  onSelect: () => logout()
}]])
</script>

<template>
  <UDropdownMenu
    :items="items"
    :content="{ align: 'center', collisionPadding: 12 }"
    :ui="{ content: collapsed ? 'w-48' : 'w-(--reka-dropdown-menu-trigger-width)' }"
  >
    <UButton
      v-bind="{
        ...account,
        label: collapsed ? undefined : account.name,
        trailingIcon: collapsed ? undefined : 'i-lucide-chevrons-up-down'
      }"
      color="neutral"
      variant="ghost"
      block
      :square="collapsed"
    />
  </UDropdownMenu>
</template>
