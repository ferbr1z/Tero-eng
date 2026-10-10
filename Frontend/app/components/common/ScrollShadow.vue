<script setup lang="ts">
const props = withDefaults(defineProps<{
  maxHeight?: string
  // Debe coincidir con el fondo del contenedor para integrar el degradado.
  fadeClass?: string
}>(), {
  maxHeight: '16rem',
  fadeClass: 'bg-default'
})

const scrollEl = ref<HTMLElement | null>(null)
const contentEl = ref<HTMLElement | null>(null)
const canScrollUp = ref(false)
const canScrollDown = ref(false)

function updateShadows() {
  const el = scrollEl.value
  if (!el) return
  canScrollUp.value = el.scrollTop > 0
  canScrollDown.value = Math.ceil(el.scrollTop + el.clientHeight) < el.scrollHeight
}

let observer: ResizeObserver | undefined

onMounted(() => {
  updateShadows()
  observer = new ResizeObserver(updateShadows)
  if (scrollEl.value) observer.observe(scrollEl.value)
  if (contentEl.value) observer.observe(contentEl.value)
})

onBeforeUnmount(() => observer?.disconnect())
</script>

<template>
  <div class="relative">
    <div
      ref="scrollEl"
      class="overflow-y-auto"
      :style="{ maxHeight: props.maxHeight }"
      @scroll="updateShadows"
    >
      <div ref="contentEl">
        <slot />
      </div>
    </div>
    <div
      v-show="canScrollUp"
      :class="fadeClass"
      class="scroll-shadow-fade scroll-shadow-fade--top pointer-events-none absolute inset-x-0 top-0 h-8"
    />
    <div
      v-show="canScrollDown"
      :class="fadeClass"
      class="scroll-shadow-fade scroll-shadow-fade--bottom pointer-events-none absolute inset-x-0 bottom-0 h-8"
    />
  </div>
</template>

<style scoped>
.scroll-shadow-fade--top {
  mask-image: linear-gradient(to bottom, black 0%, black 35%, transparent 100%);
  -webkit-mask-image: linear-gradient(to bottom, black 0%, black 35%, transparent 100%);
}

.scroll-shadow-fade--bottom {
  mask-image: linear-gradient(to top, black 0%, black 35%, transparent 100%);
  -webkit-mask-image: linear-gradient(to top, black 0%, black 35%, transparent 100%);
}
</style>
