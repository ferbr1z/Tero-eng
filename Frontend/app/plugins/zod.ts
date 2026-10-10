import * as z from 'zod'
import { es } from 'zod/locales'

export default defineNuxtPlugin(() => {
  const spanish = es()

  // Traduce los errores predeterminados y evita el mensaje técnico
  // "Invalid input" cuando un control entrega un tipo de dato incorrecto.
  z.config({
    ...spanish,
    localeError: (issue) => {
      if (issue.code === 'invalid_type') {
        const messages: Record<string, string> = {
          number: 'Ingresa un número válido.',
          string: 'Ingresa un texto válido.',
          boolean: 'Selecciona una opción válida.',
          array: 'Selecciona una opción válida.'
        }

        return messages[issue.expected] ?? 'El valor ingresado no es válido.'
      }

      return spanish.localeError(issue)
    }
  })
})
