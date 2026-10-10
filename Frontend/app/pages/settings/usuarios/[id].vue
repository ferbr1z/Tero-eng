<script setup lang="ts">
const route = useRoute()
const { getUsuario } = useUsuarios()
const { data: usuario, error } = await useAsyncData(`usuario-${route.params.id}`, () => getUsuario(Number(route.params.id)))

if (error.value || !usuario.value) throw createError({ statusCode: 404, statusMessage: 'Usuario no encontrado' })
</script>

<template>
  <SettingsUsuarioForm :usuario="usuario!" />
</template>
