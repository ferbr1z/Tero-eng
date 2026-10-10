<script setup lang="ts">
import * as z from 'zod'
import type { FormSubmitEvent } from '@nuxt/ui'
import type { Usuario, UsuarioAddRequest, UsuarioRequest } from '~/types'
import { ALL_PERMISOS, PERMISO_GRUPOS, ROLES } from '~/constants/permisos'

const props = defineProps<{ usuario?: Usuario }>()

const schema = z.object({
  nombre: z.string().min(1, 'El nombre es requerido'),
  email: z.string().email('El email no es válido').optional().or(z.literal('')).transform(value => value || undefined),
  numDocumento: z.string().min(1, 'El documento es requerido'),
  telefono: z.string().optional().or(z.literal('')).transform(value => value || undefined),
  direccion: z.string().optional(),
  password: z.string().min(6, 'La contraseña debe tener al menos 6 caracteres').regex(/\d/, 'La contraseña debe incluir al menos un número').optional(),
  rol: z.preprocess(value => value ?? '', z.string().min(1, 'Selecciona un rol')),
  permisos: z.array(z.string())
}).superRefine((value, ctx) => {
  if (!props.usuario && !value.password) {
    ctx.addIssue({ code: 'custom', path: ['password'], message: 'La contraseña es requerida' })
  }
  if (!value.email && !value.telefono) {
    ctx.addIssue({ code: 'custom', path: ['email'], message: 'Ingresa un email o teléfono' })
  }
})

type Schema = z.output<typeof schema>

const state = reactive<Partial<Schema>>({
  nombre: props.usuario?.nombre ?? '',
  email: props.usuario?.email ?? '',
  numDocumento: props.usuario?.numDocumento ?? '',
  telefono: props.usuario?.telefono ?? '',
  direccion: props.usuario?.direccion ?? '',
  password: undefined,
  rol: props.usuario?.roles[0] ?? '',
  permisos: []
})

const toast = useToast()
const router = useRouter()
const { createUsuario, updateUsuario, getPermisos, getPermisosRol, setPermisos } = useUsuarios()
const loading = ref(false)
const isEdit = computed(() => !!props.usuario)
const roleOptions = ROLES.map(role => ({ label: role.nombre, value: role.nombre as string }))
const permissionGroups = PERMISO_GRUPOS.map(grupo => ({
  name: grupo.name,
  permisos: grupo.permisos.map(permiso => ({ value: permiso.value as string, label: permiso.label }))
}))
function setPermissions(permisos: readonly string[]) {
  state.permisos = [...permisos]
}

function isGroupSelected(groupPermisos: { value: string }[]) {
  return groupPermisos.length > 0 && groupPermisos.every(permiso => state.permisos?.includes(permiso.value))
}

function isGroupIndeterminate(groupPermisos: { value: string }[]) {
  const selected = groupPermisos.filter(permiso => state.permisos?.includes(permiso.value)).length
  return selected > 0 && selected < groupPermisos.length
}

function toggleGroup(groupPermisos: { value: string }[], value: boolean | 'indeterminate') {
  const ids = groupPermisos.map(permiso => permiso.value)
  const current = state.permisos ?? []
  setPermissions(value === true
    ? [...new Set([...current, ...ids])]
    : current.filter(permiso => !ids.includes(permiso)))
}

function updateGroupPermissions(groupPermisos: { value: string }[], selected: string[]) {
  const ids = groupPermisos.map(permiso => permiso.value)
  const selectedInGroup = selected.filter(permiso => ids.includes(permiso))
  setPermissions([
    ...(state.permisos ?? []).filter(permiso => !ids.includes(permiso)),
    ...selectedInGroup
  ])
}

onMounted(async () => {
  if (!props.usuario) return
  try {
    setPermissions(await getPermisos(props.usuario.id))
  } catch (error) {
    toast.add({ title: 'No se pudieron cargar los datos del usuario', description: getApiErrorMessage(error), color: 'error' })
  }
})

watch(() => state.rol, async (rol) => {
  const selectedRole = ROLES.find(item => item.nombre === rol)
  if (!selectedRole) return

  try {
    setPermissions(await getPermisosRol(selectedRole.nombre))
  } catch (error) {
    toast.add({ title: 'No se pudieron cargar los permisos del rol', description: getApiErrorMessage(error), color: 'error' })
  }
})

async function onSubmit(event: FormSubmitEvent<Schema>) {
  loading.value = true
  try {
    const { password, permisos, rol, ...request } = event.data
    const requestWithRole = { ...request, roles: [rol] }
    const saved = isEdit.value && props.usuario
      ? await updateUsuario(props.usuario.id, requestWithRole satisfies UsuarioRequest)
      : await createUsuario({ ...requestWithRole, password: password! } satisfies UsuarioAddRequest)

    await setPermisos(saved.id, Object.fromEntries(ALL_PERMISOS.map(permiso => [permiso, permisos.includes(permiso)])))
    toast.add({ title: isEdit.value ? 'Usuario actualizado' : 'Usuario creado', color: 'success' })
    await router.push('/settings/usuarios')
  } catch (error) {
    toast.add({ title: 'No se pudo guardar el usuario', description: getApiErrorMessage(error), color: 'error' })
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <UForm
    :schema="schema"
    :state="state"
    class="space-y-6"
    @submit="onSubmit"
  >
    <UPageCard
      :title="isEdit ? 'Editar usuario' : 'Nuevo usuario'"
      description="Define los datos de acceso, el rol y los permisos individuales."
      variant="naked"
      orientation="horizontal"
    >
      <div class="flex w-fit gap-2 lg:ms-auto">
        <UButton
          label="Cancelar"
          color="neutral"
          variant="outline"
          to="/settings/usuarios"
        />
        <UButton :label="isEdit ? 'Guardar cambios' : 'Crear usuario'" type="submit" :loading="loading" />
      </div>
    </UPageCard>

    <UPageCard title="Datos personales" variant="subtle">
      <div class="grid gap-4 md:grid-cols-2">
        <UFormField label="Nombre" name="nombre" required>
          <UInput v-model="state.nombre" class="w-full" />
        </UFormField>
        <UFormField label="Documento" name="numDocumento" required>
          <UInput v-model="state.numDocumento" class="w-full" />
        </UFormField>
        <UFormField label="Email" name="email">
          <UInput v-model="state.email" type="email" class="w-full" />
        </UFormField>
        <UFormField label="Teléfono" name="telefono">
          <UInput v-model="state.telefono" class="w-full" />
        </UFormField>
        <UFormField label="Dirección" name="direccion" class="md:col-span-2">
          <UInput v-model="state.direccion" class="w-full" />
        </UFormField>
        <UFormField
          :label="isEdit ? 'Nueva contraseña (opcional)' : 'Contraseña'"
          name="password"
          :required="!isEdit"
          class="md:col-span-2"
        >
          <UInput
            v-model="state.password"
            type="password"
            autocomplete="new-password"
            class="w-full"
          />
        </UFormField>
      </div>
    </UPageCard>

    <UPageCard title="Acceso" description="Al seleccionar un rol se cargan sus permisos; puedes ajustarlos para este usuario." variant="subtle">
      <UFormField label="Rol" name="rol" required>
        <USelectMenu
          v-model="state.rol"
          :items="roleOptions"
          value-key="value"
          class="w-full"
          placeholder="Selecciona un rol"
          clear
        />
      </UFormField>
      <USeparator class="my-5" />
      <UFormField label="Permisos individuales" name="permisos">
        <div class="grid gap-4 md:grid-cols-2">
          <div v-for="group in permissionGroups" :key="group.name" class="rounded-lg border border-default p-4">
            <UCheckbox
              :model-value="isGroupIndeterminate(group.permisos) ? 'indeterminate' : isGroupSelected(group.permisos)"
              :label="`Todos los permisos de ${group.name}`"
              class="mb-3 font-medium text-highlighted"
              @update:model-value="value => toggleGroup(group.permisos, value)"
            />
            <USeparator class="mb-3" />
            <UCheckboxGroup
              :model-value="state.permisos ?? []"
              :items="group.permisos"
              @update:model-value="selected => updateGroupPermissions(group.permisos, selected)"
            />
          </div>
        </div>
      </UFormField>
    </UPageCard>
  </UForm>
</template>
