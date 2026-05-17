import { fr } from './fr'
import type { AppResource } from './types'

const resources: Record<string, AppResource> = {
  fr,
}

export function getResource(locale = 'fr'): AppResource {
  return resources[locale] ?? resources.fr
}

export { resources }