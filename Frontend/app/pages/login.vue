<script setup lang="ts">
import * as z from 'zod'
import type { FormSubmitEvent } from '@nuxt/ui'

definePageMeta({
  layout: false
})

const loginSchema = z.object({
  identifier: z.string().min(1, 'Ingresa tu email o teléfono'),
  password: z.string().min(1, 'La contraseña es requerida')
})

type LoginSchema = z.output<typeof loginSchema>

const credentials = reactive<Partial<LoginSchema>>({
  identifier: '',
  password: ''
})

const loading = ref(false)
const toast = useToast()
const { login } = useAuth()

async function onSubmit(event: FormSubmitEvent<LoginSchema>) {
  loading.value = true
  try {
    const isEmail = event.data.identifier.includes('@')
    await login({
      email: isEmail ? event.data.identifier : undefined,
      telefono: isEmail ? undefined : event.data.identifier,
      password: event.data.password
    })
    await navigateTo('/')
  } catch (error) {
    toast.add({
      title: 'No se pudo iniciar sesión',
      description: getApiErrorMessage(error, 'Credenciales inválidas'),
      color: 'error'
    })
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="flex min-h-screen items-center justify-center bg-default p-4">
    <UPageCard title="Iniciar sesión" description="Ingresa tus credenciales para acceder." class="w-full max-w-sm">
      <UForm
        :schema="loginSchema"
        :state="credentials"
        class="flex flex-col gap-4"
        @submit="onSubmit"
      >
        <UFormField label="Email o teléfono" name="identifier" required>
          <UInput
            v-model="credentials.identifier"
            type="text"
            placeholder="tu@email.com o tu teléfono"
            class="w-full"
          />
        </UFormField>

        <UFormField label="Contraseña" name="password" required>
          <UInput
            v-model="credentials.password"
            type="password"
            placeholder="••••••••"
            class="w-full"
          />
        </UFormField>

        <UButton
          label="Iniciar sesión"
          type="submit"
          block
          :loading="loading"
        />
      </UForm>
    </UPageCard>
  </div>
</template>
