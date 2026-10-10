<script setup lang="ts">
const pageSize = defineModel<number>({ required: true })

withDefaults(defineProps<{ options?: number[] }>(), {
  options: () => [10, 25, 50]
})
</script>

<template>
  <div class="flex items-center gap-2">
    <span class="text-sm text-muted hidden md:block">Filas por página</span>
    <USelectMenu
      v-model="pageSize"
      :items="options"
      size="sm"
      class="w-20"
    />
  </div>
</template>
